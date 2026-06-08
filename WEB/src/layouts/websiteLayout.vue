<template>
  <q-layout view="hHh lpR lFf">
    <div>
      <!-- HEADER -->
      <q-header class="custom-navbar" :class="headerClass">
        <q-toolbar
          class="q-py-none items-center justify-between"
          style="padding-left: 10%; padding-right: 10%"
        >
          <!-- Logo -->
          <div class="row items-center">
            <img
              src="~assets/website/home/Meldep Logo.png"
              alt="Logo"
              style="height: 60px"
              @click="$router.push('/')"
            >
          </div>

          <!-- Desktop Navigation -->
          <div class="row items-right q-gutter-lg q-pt-xs hide-on-mobile">
            <q-btn
              flat
              label="Home"
              class="text-weight-bold text-capitalize"
              color="black"
              style="font-size: 16px"
              @click="$router.push('/')"
            />

            <q-btn
              label="Book a demo"
              text-color="black"
              class="custom-btn"
              unelevated
              style="border: 1px solid #a6a6a6; background: transparent"
              @click="$router.push('/demo')"
            />

            <q-btn
              label="Contact Us"
              text-color="black"
              class="custom-btn"
              unelevated
              style="border: 1px solid #a6a6a6; background: transparent"
              @click="$router.push('/contactUs')"
            />

            <q-btn
              unelevated
              label="Login"
              color="black"
              class="text-white text-capitalize custom-btn"
              @click="$router.push('/auth/login')"
            />
          </div>

          <!-- Mobile Menu Button -->
          <q-btn
            dense
            flat
            round
            icon="o_menu"
            color="black"
            class="menu-toggle-btn"
            @click="toggleDrawer"
          />
        </q-toolbar>
      </q-header>

      <!-- MOBILE DRAWER -->
      <q-drawer
        v-model="leftDrawerOpen"
        side="right"
        overlay
        behavior="mobile"
        class="bg-white text-black"
      >
        <q-list class="q-pa-md">
          <q-item v-ripple clickable class="q-mb-sm" @click="$router.push('/')">
            <q-item-section>Home</q-item-section>
          </q-item>

          <q-item v-ripple clickable class="q-mb-sm" @click="$router.push('/demo')">
            <q-item-section>Book a Demo</q-item-section>
          </q-item>

          <q-item v-ripple clickable class="q-mb-sm" @click="$router.push('/contactUs')">
            <q-item-section>Contact Us</q-item-section>
          </q-item>

          <q-item v-ripple clickable class="q-mb-sm" @click="$router.push('/auth/login')">
            <q-item-section>Login</q-item-section>
          </q-item>
        </q-list>
      </q-drawer>
    </div>

    <q-page-container class="container-flat q-mt-xl">
      <router-view />
    </q-page-container>

    <q-footer
      class="footer-background q-mt-md"
    >
      <div
        class="column justify-between"
      >
        <div class="row q-col-gutter-xl q-mb-xl q-mt-xl line">
          <!-- Logo -->
          <div class="col-12 col-md-4 flex items-center justify-center justify-md-start">
            <q-img
              src="~assets/website/home/MeldEPlogo1.png"
              style="max-height: 150px; width: 220px"
              fit="contain"
              @click="$router.push('/')"
            />
          </div>

          <!-- Navigation Links -->
          <div class="col-12 col-md-4 flex justify-center footer">
            <div class="row q-gutter-md justify-center">
              <p
                class="text-grey-1 text-weight-thin q-px-sm cursor-pointer"
                style="text-decoration: none; font-size: 16px"
                @click="$router.push('/')"
              >
                Home
              </p>
              <p
                class="text-grey-1 text-weight-thin q-px-sm cursor-pointer"
                style="text-decoration: none; font-size: 16px" @click="$router.push('/auth/login')"
              >
                Log In
              </p>
              <p
                class="text-grey-1 text-weight-thin q-px-sm cursor-pointer"
                style="text-decoration: none; font-size: 16px"
                @click="$router.push('/demo')"
              >
                Book Your Demo
              </p>
              <p
                class="text-grey-1 text-weight-thin q-px-sm cursor-pointer"
                style="text-decoration: none; font-size: 16px"
                @click="$router.push('/contactUs')"
              >
                Contact Us
              </p>
            </div>
          </div>

          <!-- Social Icons -->
          <div class="col-12 col-md-4 flex justify-center justify-xl-end footer">
            <div class="row q-gutter-lg">
              <a href="https://www.facebook.com/people/MeldEp/61572614593655/" target="_blank">
                <q-icon name="mdi-facebook" size="20px" class="cursor-pointer text-white q-mr-sm" />
              </a>
              <a href="https://www.linkedin.com/company/meld-ep/" target="_blank">
                <q-icon name="mdi-linkedin" size="20px" class="cursor-pointer text-white q-mr-sm" />
              </a>
              <a href="https://www.instagram.com/meldep25" target="_blank">
                <q-icon name="mdi-instagram" size="20px" class="cursor-pointer text-white" />
              </a>

            </div>
          </div>
        </div>

        <!-- Divider -->
        <q-separator color="grey" class="line" />

        <!-- Bottom Text -->
        <div class="row q-mt-md items-center">
          <div class="col-12 col-md-5 text-caption text-center text-grey-5">
            Copyrights © 2025. All Rights Reserved
          </div>
          <div class="col-12 col-md-4" />
          <div class="col-12 col-md-3 text-caption text-center text-grey-5">
            Developed by VSky Solutions
          </div>
        </div>
      </div>
    </q-footer>
  </q-layout>
</template>

<script setup>
import { ref, computed } from "vue";
import { useQuasar } from "quasar";

const $q = useQuasar();
const leftDrawerOpen = ref(false);

const toggleDrawer = () => {
  leftDrawerOpen.value = !leftDrawerOpen.value;
};

const headerClass = computed(() => {
  return $q.screen.gt.xs ? "q-px-xl" : "q-px-md";
});

</script>
