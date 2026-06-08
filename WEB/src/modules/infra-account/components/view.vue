<template>
  <q-dialog ref="dialogRef" class="customDialog" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 75vw !important;max-width: 75vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white" style="flex-grow: 1;">{{ model.name }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <q-tabs v-model="tab" dense class="text-primary" active-color="primary" indicator-color="primary" active-class="bg-blue-1 borderRadiusTabs" align="left" narrow-indicator>
            <q-tab name="1_tab" label="Account Info." class="q-px-lg q-mr-md" />
            <q-tab name="2_tab" label="Account Services" class="q-px-lg" />
            <q-tab name="3_tab" label="FTP" class="q-px-lg q-mr-md" />
            <q-tab name="4_tab" label="Database" class="q-px-lg" :disable="disableTab" />
          </q-tabs>
          <q-separator />
          <q-tab-panels v-model="tab" animated class="q-mt-xs">
            <q-tab-panel name="1_tab">
              <fieldset>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-md-6">
                    <div class="q-mb-xs">Name</div>
                    <div class="text-black q-mb-cs">
                      {{ model.name }}
                    </div>
                  </div>
                  <div class="col">
                    <div class="col-12 col-md-6">Provider</div>
                    <div class="text-black">
                      {{ model.provider.dropDownValue || '-' }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-md-6">
                    <div class="q-mb-xs">Wallet Type</div>
                    <div class="text-black q-mb-cs">
                      {{ model.walletType.dropDownValue || '-' }}
                    </div>
                  </div>
                  <div class="col-12 col-md-6">
                    <div class="q-mb-xs">Wallet Number</div>
                    <div class="text-black">
                      {{ model.walletNumber || '-' }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-md-6">
                    <div class="q-mb-xs">Customer Id</div>
                    <div class="text-black">
                      {{ model.customerId }}
                    </div>
                  </div>
                  <div class="col-12 col-md-6">
                    <div class="q-mb-xs">Credit Card Last 4 Digits</div>
                    <div class="text-black">
                      {{ model.ccLast4Digits }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-md-12">
                    <div class="q-mb-xs">URL</div>
                    <div class="text-black">
                      <a
                        :href="model.url"
                        target="_blank"
                        class="link-text"
                      >
                        {{ model.url }}
                      </a>
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-md-12">
                    <div class="q-mb-xs">Instructions</div>
                    <div class="text-black RichTextEditor">
                      <p v-html="model.instructions ? model.instructions : '-'" />
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Created By</div>
                    <div class="text-black">
                      {{ model.createdBy.person.fullName ? model.createdBy.person.fullName : "-" }}
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Created Date</div>
                    <div class="text-black">
                      {{ model.createdOnUtc ? model.createdOnUtc : "-" }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Updated By</div>
                    <div class="text-black">
                      {{ model.updatedBy.person.fullName ? model.updatedBy.person.fullName : "-" }}
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Updated Date</div>
                    <div class="text-black">
                      {{ model.updatedOnUtc ? model.updatedOnUtc : "-" }}
                    </div>
                  </div>
                </div>
              </fieldset>
            </q-tab-panel>
            <q-tab-panel name="2_tab">
              <AccountServicesTab
                :rows="rows"
                :is-show="false"
                :loading="loading"
              />
            </q-tab-panel>
            <q-tab-panel name="3_tab">
              <FtpTab
                :rows="ftpRows"
                :loading="loading"
              />
            </q-tab-panel>
            <q-tab-panel name="4_tab">
              <DatabaseTab
                :rows="databaseRows"
                :loading="loading"
              />
            </q-tab-panel>
          </q-tab-panels>
        </div>
      </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { ref, onMounted } from "vue";
import infraAccountService from "../infraAccount.service";
import AccountServicesTab from "modules/infra-account/components/_accountServicesTab.vue";
import FtpTab from "modules/infra-account/components/_ftpTab.vue";
import DatabaseTab from "modules/infra-account/components/_databaseTab.vue";

// Common variables
const loading = ref(true);
const rows = ref([]);
const ftpRows = ref([]);
const databaseRows = ref([]);
const tab = ref("1_tab");
// const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Define model values
const model = ref({
  name: "-",
  walletNumber: "",
  url: "",
  customerId: "",
  ccLast4Digits: "",
  instructions: "",
  provider: {
    dropDownValue: ""
  },
  walletType: {
    dropDownValue: ""
  },
  createdBy: {
    person: {
      fullName: ""
    }
  },
  updatedBy: {
    person: {
      fullName: ""
    }
  }
});

const getInfraAccount = async () => {
  try {
    loading.value = true;
    const resp = await infraAccountService.getInfraAccountDetails(props.id);
    model.value = {
      ...resp
    };
    rows.value = (resp.infraAccountServices ?? []).map(service => ({
      ...service,
      itemTypeId: service.itemType?.id ?? null,
      ownerShipTypeId: service.ownerShipType?.id ?? null,
      paymentTermId: service.paymentTerm?.id ?? null,
      walletTypeId: service.walletType?.id ?? null,
      startDateStr: service.startDate ?? null,
      instructions: service.instructions ?? null,
      deleted: false
    }));
    // FTP List
    ftpRows.value = (resp.infraAccountServices ?? [])
      .flatMap(service => service.infraFTPList ?? [])
      .map(ftp => ({
        ...ftp,
        protocolTypeId: ftp.protocolType?.id ?? null,
        encryptionTypeId: ftp.encryptionType?.id ?? null,
        walletTypeId: ftp.walletType?.id ?? null,
        deleted: false
      }));

    // Database List
    databaseRows.value = (resp.infraAccountServices ?? [])
      .flatMap(service => service.infraDatabaseList ?? [])
      .map(db => ({
        ...db,
        walletTypeId: db.walletType?.id ?? null,
        deleted: false
      }));
  } catch (error) {
    console.error("Failed to load Infra Account:", error);
  } finally {
    loading.value = false;
  }
};

// const totalPrice = computed(() => {
//   return rows.value.reduce((sum, row) => {
//     const price = parseFloat(row.priceInDollar) || 0;
//     return sum + price;
//   }, 0);
// });

// On page rendering
onMounted(() => {
  getInfraAccount();
});

</script>
<style>
.ellipsis-cell {
  max-width: 260px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>
