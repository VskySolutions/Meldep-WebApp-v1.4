<template>
  <div class="q-pa-md q-gutter-y-md">
    <q-banner rounded class="bg-primary text-white text-center">
      <div class="row items-center justify-between">
        <!-- Centered Welcome Text -->
        <div class="col text-center">
          <h1>Welcome To Meldep</h1>
        </div>
        <div>
          <q-btn color="red" label="Login" class="cursor-pointer q-mr-sm" @click="$router.push('/auth/login')" />
        </div>
      </div>
    </q-banner>
  </div>
</template>
