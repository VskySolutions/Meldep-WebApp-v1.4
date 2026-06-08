<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools flex items-center justify-between">
        <q-breadcrumbs class="text-brown text-weight-bold text-h3">
          <template #separator>
            <q-icon size="1.5em" name="o_chevron_right" color="primary" />
          </template>
          <q-breadcrumbs-el label="Settings" />
        </q-breadcrumbs>
      </q-card-section>
      <q-separator class="q-mb-lg" />
      <q-card class="flex flex-center q-pa-sm">
        <q-card-section>
          <div class="row q-col-gutter-md q-ml-md">
            <q-card
              v-for="(menu) in filteredMenus"
              :key="menu.id" class="col-xs-12 col-sm-6 col-md-4 col-lg-2 text-center q-ml-xl q-my-md q-py-md custom-card cursor-pointer"
              @click="$router.push(menu.link)"
            >
              <q-icon
                :name="menu.icon"
                size="60px"
                class="text-primary q-pr-lg"
              />
              <div
                class="text-h1 text-bold text-primary q-pr-lg"
              >
                {{ menu.displayName }}
              </div>
            </q-card>
          </div>
        </q-card-section>
      </q-card>
    </q-card>
  </q-page>
</template>
<script setup>
import { ref, onMounted, computed } from "vue";
import moduleService from "modules/module/module.service";
import { useAuthStore } from "stores/auth";

const authStore = useAuthStore();
const user = authStore.user;
const roles = computed(() => user?.roles || []);
const settingMenus = ref([]);

const getSiteActiveModulesMenus = () => {
  moduleService.getSiteActiveModulesMenus().then((resp) => {
    // Find the Settings module
    const settingModule = resp.find(m => m.name?.includes("Settings"));
    settingMenus.value = settingModule?.customSiteModuleMenuList || [];
  });
};

const filteredMenus = computed(() => {
  const isAdmin = roles.value.includes("system-super-admin");
  return settingMenus.value.filter((menu) => {
    if (menu.displayName === "Site Roles" && isAdmin) {
      return false; // hide Site Roles if system-super-admin
    }
    if (["Sites", "Modules", "Master Roles"].includes(menu.displayName)) {
      return isAdmin; // only show these if system-super-admin
    }
    return true;
  });
});

onMounted(() => {
  getSiteActiveModulesMenus();
});
</script>
<style>
</style>
