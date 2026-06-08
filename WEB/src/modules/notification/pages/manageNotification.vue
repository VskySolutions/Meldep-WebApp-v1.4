<template>
  <q-page padding>
    <div class="row q-col-gutter-md">
      <div class="col-12 col-md-2">
        <q-card class="full-height">
          <q-card-section>
            <q-breadcrumbs class="text-brown text-weight-bold text-h3 q-mt-sm q-mb-md">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Notification Channels" />
            </q-breadcrumbs>
            <q-list bordered separator>
              <q-item
                clickable
                :active="activeComponent === 'system'"
                active-class="bg-blue-1 text-primary"
                @click="activeComponent = 'system'"
              >
                <q-item-section>
                  System Notifications
                </q-item-section>
                <!-- Arrow -->
                <q-item-section side v-if="activeComponent === 'system'">
                  <q-icon name="o_keyboard_double_arrow_right" />
                </q-item-section>
              </q-item>
              <q-item
                clickable
                :active="activeComponent === 'email'"
                active-class="bg-blue-1 text-primary"
                @click="activeComponent = 'email'"
              >
                <q-item-section>
                  Email Notifications
                </q-item-section>
                <!-- Arrow -->
                <q-item-section side v-if="activeComponent === 'email'">
                  <q-icon name="o_keyboard_double_arrow_right" />
                </q-item-section>
              </q-item>
            </q-list>
          </q-card-section>
        </q-card>
      </div>
      <div class="col-12 col-md-10">
        <div class="full-height">
          <manageSystemNotifications
            v-if="activeComponent === 'system'"
          />
          <manageEmailNotifications
            v-if="activeComponent === 'email'"
          />
          <div v-if="!activeComponent" class="text-grey text-center q-pa-lg">
            Please select an option.
          </div>
        </div>
      </div>
    </div>
  </q-page>
</template>

<script setup>
import { ref } from "vue";
import manageSystemNotifications from "modules/notification/components/_manageSystemNotifications.vue";
import manageEmailNotifications from "modules/notification/components/_manageEmailNotifications.vue";
const activeComponent = ref("system");
</script>
