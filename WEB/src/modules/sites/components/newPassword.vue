<template>
  <q-dialog ref="dialogRef" persistent @hide="onDialogHide">
    <q-card class="q-dialog-plugin dialog-sm">
      <q-card-section class="card-header with-tools">
        <div class="text-h2">New Password - {{ props.item?.password }}</div>
        <q-btn v-close-popup icon="o_close" class="close" flat round dense />
      </q-card-section>
      <q-separator />
      <q-card-section class="card-body scroll">
        <p class="text-body2">
          Here is your app password.
          Please ensure to note it down, as you won't be able to retrieve it once you close this window.
        </p>
        <q-field filled stack-label :dense="false">
          <template #control>
            <!-- <div class="self-center full-width no-outline fs-24">{{ props.item?.password }}</div> -->
          </template>
        </q-field>
      </q-card-section>
    </q-card>
  </q-dialog>
</template>

<script setup>
// import { ref } from "vue";
import { useDialogPluginComponent } from "quasar";

defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

const props = defineProps({ item: { type: Object, default: null } });
// console.log(props);

// const model = ref({});

// onMounted(() => {
//   onReset();
// });

// async function onReset () {
//   userService.resetPassword(props.id).then(resp => {
//     model.value.password = resp.password;
//     notifySuccess({ message: "Password is updated successfully." });
//   });
// }
</script>
