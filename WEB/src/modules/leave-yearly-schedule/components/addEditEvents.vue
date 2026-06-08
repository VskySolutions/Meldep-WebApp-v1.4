<template>
  <q-dialog
    ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right"
    @hide="onDialogHide"
  >
    <q-card
      class="q-dialog-plugin PersonMain card-header with-tools headerBasic"
      style="width: 50vw; max-width: 50vw;"
    >
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Leave</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <div class="row q-col-gutter-x-md q-mb-md q-pb-sm">
                <div class="col-lg-4 col-md-4 col-sm-4 col-xs-6 col-12 q-mb-md">
                  <label class="label q-mb-xs text-black">Date<span class="required">*</span></label>
                  <div>
                    <q-input v-model="model.date" outlined stack-label hide-bottom-space mask="##/##/####" dense :error="v$.date.$error" :error-message="v$.date.$errors[0]?.$message" @blur="v$.date.$touch">
                      <template #append>
                        <q-icon name="o_calendar_month" class="cursor-pointer">
                          <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                            <q-date
                              v-model="model.date" mask="MM/DD/YYYY"
                              @update:model-value="() => $refs.qDateProxy.hide()"
                            />
                          </q-popup-proxy>
                        </q-icon>
                      </template>
                    </q-input>
                  </div>
                </div>
                <div class="col-lg-8 col-md-8 col-sm-8 col-xs-6 col-12 q-mb-md">
                  <label class="label q-mb-xs text-black">Title<span class="required">*</span></label>
                  <q-input v-model="model.title" outlined stack-label hide-bottom-space :dense="true" maxlength="500" autofocus :error="v$.title.$error" :error-message="v$.title.$errors[0]?.$message" @blur="v$.title.$touch" />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md q-pb-sm">
                <div class="col-12 q-mb-md">
                  <div class="label q-mb-xs text-black"><label>Description</label></div>
                  <div class="form-group">
                    <q-editor
                      v-model="model.description" :dense="$q.screen.lt.md" :toolbar="[
                        [
                          {
                            label: $q.lang.editor.align,
                            icon: $q.iconSet.editor.align,
                            fixedLabel: true,
                            list: 'only-icons',
                            options: ['left', 'center', 'right', 'justify']
                          },
                        ],
                        ['bold', 'italic', 'strike', 'underline'],
                        ['token', 'hr', 'link', 'custom_btn'],
                        [
                          {
                            label: $q.lang.editor.formatting,
                            icon: $q.iconSet.editor.formatting,
                            list: 'no-icons',
                            options: [
                              'p',
                              'h1',
                              'h2',
                              'h3',
                              'h4',
                              'h5',
                              'h6',
                              'code'
                            ]
                          },
                          'removeFormat'
                        ],
                        ['quote', 'unordered', 'ordered', 'outdent', 'indent'],

                        ['undo', 'redo'],
                        ['viewsource']
                      ]" :fonts="{
                        arial: 'Arial',
                        arial_black: 'Arial Black',
                        comic_sans: 'Comic Sans MS',
                        courier_new: 'Courier New',
                        impact: 'Impact',
                        lucida_grande: 'Lucida Grande',
                        times_new_roman: 'Times New Roman',
                        verdana: 'Verdana'
                      }"
                    />
                  </div>
                </div>
              </div>
            </fieldset>
          </div>
        </div>
        <q-card-actions class="stickyFooter q-gutter-sm justify-center">
          <q-btn
            color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps
            @click="onDialogCancel"
          />
          <q-btn
            color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing"
            no-caps
          />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { useDialogPluginComponent } from "quasar";
import { ref, watch } from "vue";
import eventLeaveService from "modules/leave-yearly-schedule/leaveYearlySchedule.service";
import { notifySuccess } from "assets/utils";
import { required, helpers } from "@vuelidate/validators";
import useVuelidate from "@vuelidate/core";
import { isDate } from "validators/zw_validators.js";
import _ from "lodash";

const $emit = defineEmits(["hide", "ok"]);
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();
const props = defineProps({
  id: { type: String, default: "" },
  selectedDate: { type: String, default: "" }, // Accept selectedDate prop
  leaveRuleId: { type: String, default: "" }
});
const loading = ref(true);
const processing = ref(false);
// Define model values
const model = ref({
  id: "",
  leaveRuleId: props.leaveRuleId,
  title: "",
  date: props.selectedDate,
  description: ""
});

const rules = {
  date: {
    required: helpers.withMessage("Date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate)
  },
  title: {
    required: helpers.withMessage("Title is required", required)
  }
};

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });
const getEvents = () => {
  if (!props.id) return; // If no ID, do nothing (Add mode)
  loading.value = true;
  eventLeaveService.getEvents(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.id = resp.id;
    model.value.leaveRuleId = resp.leaveRuleId;
    model.value.title = resp.title;
    model.value.description = resp.description;
    model.value.date = resp.date;
  }).finally(() => {
    loading.value = false;
  });
};

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getEvents();
  }
}, { immediate: true });

watch(() => props.leaveRuleId, (newValue) => {
  model.value.leaveRuleId = newValue;
}, { immediate: true });

async function onSubmit () {
  // if (!await v$.value.$validate()) {
  //   return;
  // }
  // model.value.leaveRuleId = props.leaveRuleId;
  if (await v$.value.$validate()) {
    processing.value = true;
    eventLeaveService.saveLeaveEvents(props.id, model.value).then(resp => {
      notifySuccess({ message: "Event saved successfully." });
      $emit("ok");
      $emit("hide");
    }).finally(() => {
      processing.value = false;
    });
    // setTimeout(() => {
    //   window.location.reload();
    // }, 1000);
  }
}
</script>
