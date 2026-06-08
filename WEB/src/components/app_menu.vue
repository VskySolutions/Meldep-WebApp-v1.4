<template>
  <q-list class="app-menu">
    <q-separator color="grey-8" />
    <div v-for="(singleModule) in sitesModules" :key="singleModule.id" class="flex flex-center items-evenly text-grey-8" style="max-width: 100%">
      <div v-if="singleModule.customSiteModuleMenuList.length > 0" style="width: 100%">
        <div v-if="singleModule.customSiteModuleMenuList.length === 1">
          <q-btn
            flat
            no-caps
            class="text-grey-8"
            style="width:100% !important; height: 50px"
            v-bind="singleModule.customSiteModuleMenuList[0].link.startsWith('http') ? { href: singleModule.customSiteModuleMenuList[0].link } : { to: singleModule.customSiteModuleMenuList[0].link }"
            :target="singleModule.customSiteModuleMenuList[0].openInNewTab ? '_blank' : '_self'"
          >
            {{ singleModule.customSiteModuleMenuList[0].displayName }}
          </q-btn>
          <q-separator color="grey-8" />
        </div>

        <!-- If multiple menus, keep dropdown -->
        <div v-else>
          <q-btn-dropdown
            flat
            no-caps
            :label="singleModule.name"
            class="text-grey-8"
            style="width:100% !important; height: 50px"
            header
          >
            <q-list class="app-menu">
              <q-item
                v-for="menu in singleModule.customSiteModuleMenuList"
                :key="menu.id"
                v-ripple
                clickable
                exact
                v-bind="menu.link.startsWith('http') ? { href: menu.link } : { to: menu.link }"
                :target="menu.openInNewTab ? '_blank' : '_self'"
              >
                <q-item-section avatar>
                  <q-icon
                    :name="menu.icon"
                    color="orange"
                    class="material-icons-outlined"
                  />
                </q-item-section>
                <q-item-section>
                  <q-item-label class="text-grey-8">
                    {{ menu.displayName }}
                  </q-item-label>
                </q-item-section>
              </q-item>
            </q-list>
          </q-btn-dropdown>
          <q-separator color="grey-8" />
        </div>
      </div>
    </div>
    <div v-if="['admin','site-super-admin','system-super-admin'].some(r => roles.includes(r))">
      <q-list class="app-menu">
        <q-item
          v-ripple
          clickable
          exact to="/settings"
        >
          <q-item-section>
            <q-item-label
              class="fs-13 text-center"
              style="color: rgb(88, 88, 88, 1) !important; font-weight: 500;"
            >Settings
            </q-item-label>
          </q-item-section>
        </q-item>
      </q-list>
    </div>
    <q-separator color="grey-8" />
  </q-list>
</template>

<script setup>
import { ref, onMounted } from "vue";
import moduleService from "modules/module/module.service";
import { useAuthStore } from "stores/auth";

const authStore = useAuthStore();
const user = authStore.user;
const roles = user?.roles || [];
const sitesModules = ref([]);

const getSiteActiveModulesMenus = () => {
  moduleService.getSiteActiveModulesMenus().then((resp) => {
    sitesModules.value = resp.filter(m => !m.name.includes("Settings"));
  }).finally(() => {
  });
};

onMounted(() => {
  getSiteActiveModulesMenus();
});

</script>

<style scoped>

.q-btn__content{
  display: flex;
  justify-content: space-between;
}
</style>
