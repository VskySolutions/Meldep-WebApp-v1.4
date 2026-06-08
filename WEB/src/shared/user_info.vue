<template>
  <q-btn dense flat color="grey-8">
    <div class="flex items-center nav-user-info">
      <!-- <q-tooltip v-if="rolesNames.length > 0" anchor="bottom middle" self="top middle" class="text-capitalize">
        {{ rolesNames.join(", ") }}
      </q-tooltip> -->
      <q-icon v-if="model.virtualPath" class="material-icons-outlined q-mr-lg" size="38px">
        <img :src="model.virtualPath" alt="" style="width: 100px;">
      </q-icon>
      <q-icon v-else name="o_account_circle" color="orange" class="material-icons-outlined q-mr-sm" size="38px" />
      <div class="line-height-normal q-mr-sm text-left">
        <div class="fs-16 text-capitalize flex justify-between">
          <span>{{ user.username && Array.isArray(user.username) ? user.username.join(" ") : user.username }}</span>
          <q-icon side name="o_keyboard_arrow_down" size="sm" class="q-ml-sm" style="color:#697A8D;" />
        </div>
      </div>
    </div>
    <q-menu>
      <q-list style="min-width: 250px" class="user-card">
        <q-item class="q-py-sm">
          <q-item-section avatar>
            <q-icon v-if="model.virtualPath" class="material-icons-outlined q-mr-md" size="38px">
              <img :src="model.virtualPath" alt="" style="width: 100px;">
            </q-icon>
            <q-icon v-else name="o_account_circle" color="orange" class="material-icons-outlined q-mr-sm" size="38px" />
          </q-item-section>
          <q-item-section>
            <q-item-label class="text-h3 text-capitalize" lines="2">
                {{ user.username && Array.isArray(user.username) ? user.username.join(" ") : user.username }}
                <q-icon name="o_info" color="black" class="rounded-full q-ml-sm">
                  <q-tooltip class="text-capitalize" v-if="rolesNames.length > 0">
                    {{ rolesNames.join(", ") }}
                  </q-tooltip>
                </q-icon>
            </q-item-label>
          </q-item-section>
        </q-item>
        <q-separator class="q-mb-sm" />
        <q-card v-if="!loadingSharedSites && sharedSites.length" class="q-ma-sm shared-sites-card" flat bordered >
          <!-- Header -->
          <q-card-section class="text-subtitle2 text-weight-bold">
            Switch To
          </q-card-section>

          <q-separator />
          <!-- Sites List -->
          <q-list dense>
            <q-item
              v-for="site in sharedSites"
              :key="site.sites.name"
              clickable
              class="q-py-xs"
              @click="onSiteSelected(site.siteId)"
            >
              <q-item-section avatar class="row items-center no-wrap">
                  <span :class="['dot-circle q-mr-xs hoverable-cell', site.siteId === siteId ? 'dot-active' : 'dot-inactive']"></span>
              </q-item-section>

              <q-item-section>
                <q-item-label class="text-body2">
                  {{ site.sites?.name }}
                  <span v-if="site.siteId === siteId">
                    ({{ getTimeZoneAbbreviation(user.siteTimeZone) }}<q-icon name="o_info" color="black" class="rounded-full"><q-tooltip class="text-capitalize" v-if="rolesNames.length > 0">{{ user.siteTimeZone }}</q-tooltip></q-icon>)
                  </span>
                </q-item-label>
              </q-item-section>
            </q-item>
          </q-list>
        </q-card>

        <q-item v-ripple :to="{ name: 'profile' }" clickable>
          <q-item-section avatar>
            <q-icon name="person" class="material-icons-outlined" color="orange" style="font-size: 25px;" />
            <q-icon name="image" class="material-icons-outlined" color="orange" style="font-size: 25px; margin-left: 15px; margin-top: -30px;" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Profile</q-item-label>
          </q-item-section>
        </q-item>
        <q-item v-ripple :to="{ name: 'change_password' }" clickable>
          <q-item-section avatar>
            <q-icon name="lock" color="orange" class="material-icons-outlined" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Change Password</q-item-label>
          </q-item-section>
        </q-item>
        <q-item v-ripple :to="{ name: 'help_desk' }" clickable>
          <q-item-section avatar>
            <q-icon name="support_agent" color="orange" class="material-icons-outlined" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Help Desk</q-item-label>
          </q-item-section>
        </q-item>
        <q-item
          v-if="user?.siteName === 'Vsky Solutions'"
          v-ripple
          clickable
          tag="a"
          href="https://play.google.com/store/apps/details?id=com.vsky.meldep"
          target="_blank"
        >
          <q-item-section avatar>
            <q-icon
              name="android"
              color="orange"
              class="material-icons-outlined"
            />
          </q-item-section>

          <q-item-section>
            <q-item-label>
              Download Meld-EP 4.0 Mobile App
            </q-item-label>
          </q-item-section>
        </q-item>
        <q-item v-ripple :to="{ name: 'manage_notifications' }" clickable>
          <q-item-section avatar>
            <q-icon name="edit_notifications" color="orange" class="material-icons-outlined" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Manage Notifications</q-item-label>
          </q-item-section>
        </q-item>
        <q-item v-ripple clickable @click="onLogout">
          <q-item-section avatar>
            <q-icon name="logout" color="orange" class="material-icons-outlined" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Logout</q-item-label>
          </q-item-section>
        </q-item>
      </q-list>
    </q-menu>
  </q-btn>
</template>

<script setup>
import { ref, onMounted, nextTick } from "vue";
import { useRouter } from "vue-router";
import { useAuthStore } from "stores/auth";
import { getLocalStorage } from "assets/utils";
import _ from "lodash";

import accountService from "modules/account/account.service";
import siteShareService from "modules/sites-sharing/sitesSharing.service";
import siteService from "modules/sites/site.service";

// ---------------------------------------------------------------------------------------------------
// Common variables
// ---------------------------------------------------------------------------------------------------

const router = useRouter();
const authStore = useAuthStore();
const user = authStore.user;
const rolesNames = user?.roles?.length > 0 ? user.roles : "";
const sharedSites = ref([]);
const loadingSharedSites = ref(false);

// ---------------------------------------------------------------------------------------------------
// local storage
// ---------------------------------------------------------------------------------------------------

const localStorageKey = "user";
const filterLocalStorage = getLocalStorage(localStorageKey);
const siteId = ref(filterLocalStorage?.siteId || user?.siteId || null);

const model = ref({
  virtualPath: ""
});

function getProfile () {
  accountService.getProfile().then(resp => {
    model.value = _.cloneDeep(resp);
    model.value.virtualPath = resp.picture ? resp.picture.virtualPath : "";
  });
}

const onLogout = async () => {
  authStore.user = null;
  if (user.isMsLogin) {
    authStore.msLoggedOut();
    router.push({ name: "login", params: {} });
  } else {
    authStore.logout();
    router.push({ name: "login", params: {} });
  }

  router.replace({ name: "login" });
};

const getSharedSites = async () => {
  try {
    loadingSharedSites.value = true;
    const resp = await siteShareService.getAllSharedSitesByLoggedUserId();
    sharedSites.value = resp.data;
  } catch (error) {
    console.error("Error fetching shared sites", error);
  } finally {
    loadingSharedSites.value = false;
  }
};

const getTimeZoneAbbreviation = (timeZoneName) => {
  if (!timeZoneName) return '';

  return timeZoneName
    .split(' ')
    .filter(word => word.length > 0)
    .map(word => word[0].toUpperCase())
    .join('');
};

// on change site
async function onSiteSelected (selectedId) {
  try {

    await siteShareService.updateLastUsedSite(user.userId, user.personId, selectedId);

    // Change MT
    const data = await siteService.getGlobalSiteData(selectedId);
    authStore.changeTenant(data.siteId, data.timeZone, data.name, data.landingPage, data.roles);

    // Redirect to the landing page
    if (data.landingPage) {
      await router.push(data.landingPage);
    }

    await nextTick();
    window.location.reload();
  } catch (err) {
    console.error("Error setting global site:", err);
  }
}

onMounted(() => {
  getProfile();
  getSharedSites();
});

</script>
