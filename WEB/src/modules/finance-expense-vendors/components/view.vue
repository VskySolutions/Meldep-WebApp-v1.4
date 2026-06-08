<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 1000px; height: 100% !important;max-width: 100vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ model.vendorName }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <!-- <q-card class="card-header with-tools headerBasic"> -->
          <fieldset>
            <legend>Vendor Info</legend>
            <div class="row q-col-gutter-md q-mb-md">
              <div class="col-12 col-sm-4 col-md-4">
                <div class="q-mb-xs">Vendor Name</div>
                <div class="text-black">
                  {{ model.vendorName }}
                </div>
              </div>
              <div class="col-12 col-sm-4 col-md-4">
                <div class="q-mb-xs">Phone Number</div>
                <div class="text-black">
                  {{ model.vendor_Phone }}
                </div>
              </div>
              <div class="col-12 col-sm-4 col-md-4">
                <div class="q-mb-xs">Email Address</div>
                <div class="text-black">
                  {{ model.vendor_Email }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-md q-mb-md">
              <div class="ccol-12 col-sm-4 col-md-4">
                <div class="q-mb-xs">Address 1</div>
                <div class="text-black">
                  {{ model.addressLine1 }}
                </div>
              </div>
              <div class="col-12 col-sm-4 col-md-4">
                <div class="q-mb-xs">Address 2</div>
                <div class="text-black">
                  {{ model.addressLine2 }}
                </div>
              </div>
              <div class="col-12 col-sm-4 col-md-4">
                <div class="q-mb-xs">Country</div>
                <div class="text-black">
                  {{ model.country }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-md q-mb-md">
              <div class="col-12 col-sm-4 col-md-4">
                <div class="q-mb-xs">State</div>
                <div class="text-black">
                  {{ model.stateProvince }}
                </div>
              </div>
              <div class="col-12 col-sm-4 col-md-14">
                <div class="q-mb-xs">City</div>
                <div class="text-black">
                  {{ model.city }}
                </div>
              </div>
              <div class="col-12 col-md-4">
                <div class="q-mb-xs">{{ baseCountryId == model.countryId ? 'Zip Code' : 'Pin code' }}</div>
                <div class="text-black">
                  {{ model.zipCode }}
                </div>
              </div>
            </div>
          </fieldset>
          <fieldset>
            <legend>Vendor's Owner Info</legend>
            <div class="row q-col-gutter-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Vendors Name</div>
                <div class="text-black">
                  {{ model.person.fullName }}
                </div>
              </div>
            </div>
          </fieldset>
          <fieldset>
            <legend>Bank Account Details</legend>
            <q-table
              ref="tableRef"
              virtual-scroll
              bordered
              class="no-shadow"
              :loading="loading"
              :rows="bankrows"
              :columns="bankcolumns"
              row-key="id"
              separator="cell"
              binary-state-sort
              :rows-per-page-options="[20, 50, 100, 200, 500]"
            >
              <template #header="props">
                <q-tr :props="props" class="bg-primary text-white">
                  <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                </q-tr>
              </template>
              <template #body="props">
                <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                  <q-td style="width: 10%">{{ props.row.bankName }}</q-td>
                  <q-td style="width: 10%">{{ props.row.accountNumber }}</q-td>
                  <q-td style="width: 5%">{{ props.row.ifscCode }}</q-td>
                  <q-td style="width: 5%">{{ props.row.accountType.dropDownValue }}</q-td>
                  <q-td style="width: 5%">{{ props.row.branchName }}</q-td>
                </q-tr>
              </template>
            </q-table>
          </fieldset>
          <fieldset v-if="upirows.length > 0">
            <legend>UPI Details</legend>
            <q-table
              ref="tableRef"
              virtual-scroll
              bordered class="no-shadow"
              :loading="loading"
              :rows="upirows"
              :columns="upicolumns"
              row-key="id"
              separator="cell"
              binary-state-sort
              :rows-per-page-options="[20, 50, 100, 200, 500]"
            >
              <template #header="props">
                <q-tr :props="props" class="bg-primary text-white">
                  <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                </q-tr>
              </template>
              <template #body="props">
                <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                  <q-td style="width: 10%">{{ props.row.upI_ID }}</q-td>
                </q-tr>
              </template>
            </q-table>
          </fieldset>
        </div>
      </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { ref, onMounted } from "vue";
import _ from "lodash";
import expenseVendorBankAccountService from "modules/finance-expense-vendors/financeExpenseVendors.service";

// Common variables
const baseCountryId = process.env.BASE_COUNTRY_ID;
const loading = ref(true);
const bankrows = ref([]);
const upirows = ref([]);

const bankcolumns = ref([
  { name: "bankName", label: "Bank Name", field: "bankName", align: "left", sortable: true },
  { name: "accountNumber", label: "Bank Account Number", field: "accountNumber", align: "left", sortable: true },
  { name: "ifscCode", label: "IFSC Code", field: "ifscCode", align: "left", sortable: true },
  { name: "accountTypeId", label: "Account Type", field: "accountTypeId", align: "left", sortable: true },
  { name: "branchName", label: "Branch Location", field: "branchName", align: "left", sortable: true }
]);

const upicolumns = ref([
  { name: "upI_ID", label: "UPI Number", field: "upI_ID", align: "left", sortable: true }
]);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Define model values
const model = ref({
  person: {
    fullName: ""
  },
  country: null,
  countryId: null
});
// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// get project details
const getVendor = () => {
  loading.value = true;
  expenseVendorBankAccountService.getVendor(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    if (resp.address) {
      model.value.addressLine1 = resp.address.addressLine1;
      model.value.addressLine2 = resp.address.addressLine2;
      model.value.countryId = resp.address.countryId;
      model.value.country = resp.address.addressCountry.name;
      model.value.stateProvince = resp.address.addressStateProvince.name;
      model.value.city = resp.address.city;
      model.value.zipCode = resp.address.zipCode;
    }
    bankrows.value = resp.expenseVendorBankAccounts.filter(item => item.isBankAccount).map(item => ({
      ...item,
      editing: false,
      isBankAccount: item.isBankAccount,
      flag: "Edit"
    }));
    upirows.value = !resp.expenseVendorBankAccounts.isBankAccount ? resp.expenseVendorBankAccounts.filter(item => !item.isBankAccount && item.paymentType.dropDownValue !== "By Cash").map(item => ({
      ...item,
      editing: false,
      isBankAccount: item.isBankAccount,
      flag: "Edit"
    })) : [];
    model.value.isBankAccount = bankrows.value.length > 0;
  }).finally(() => {
    loading.value = false;
  });
};

// On page rendering
onMounted(() => {
  getVendor();
});
</script>
