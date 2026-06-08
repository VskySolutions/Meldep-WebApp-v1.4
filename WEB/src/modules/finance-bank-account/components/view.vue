<template>
  <q-dialog ref="dialogRef" class="customDialog" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:900px !important;height: 100%; max-width: 100vw !important;">
      <!-- Header Section -->
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ model.bankName }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <!-- Bank Account Details -->
          <fieldset>
            <legend>Bank Account Details</legend>
            <div class="row q-col-gutter-x-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Bank Name</div>
                <q-badge v-if="!model.bankName" color="red" square>No Data</q-badge>
                <div class="text-black">
                  {{ model.bankName }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mt-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Bank Account Number
                  <q-badge v-if="!model.accountNumber" color="red" square class="q-ml-sm">No Data</q-badge></div>
                <div class="text-black">
                  {{ model.accountNumber }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">IFSC Code
                  <q-badge v-if="!model.ifscCode" color="red" square class="q-ml-sm">No Data</q-badge></div>
                <div class="text-black">
                  {{ model.ifscCode }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-md q-mt-sm">
              <div class="col-6 col-sm-6">
                <div class="q-mb-xs">Account Type
                  <q-badge v-if="!model.accountTypeDropDown.dropDownValue" color="red" square class="q-ml-sm">No Data</q-badge></div>
                <div class="text-black">
                  {{ model.accountTypeDropDown.dropDownValue }}
                </div>
              </div>
              <div class="col-6 col-sm-6">
                <div class="q-mb-xs">Account Status
                  <q-badge v-if="!model.accountStatus" color="red" square class="q-ml-sm">No Data</q-badge></div>
                <div class="text-black">
                  {{ model.accountStatus }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-md q-mt-sm">
              <div class="col-6 col-sm-6">
                <div class="q-mb-xs">Branch Location
                  <q-badge v-if="!model.branchName" color="red" square class="q-ml-sm">No Data</q-badge></div>
                <div class="text-black">
                  {{ model.branchName }}
                </div>
              </div>
            </div>
          </fieldset>
          <!-- Transactions Section -->
          <fieldset>
            <legend>Recent Transactions</legend>
            <q-table
              :rows="rows"
              :columns="columns"
              row-key="id"
              flat
              bordered
              class="full-width" separator="cell"
            >
              <!-- <template #header="props">
                <q-tr :props="props" class="bg-primary text-white">
                  <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                </q-tr>
              </template>

              <template #body="props">
                <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                  <q-td v-for="col in props.cols" :key="col.name" :props="props">
                    {{ props.row[col.field] }}
                  </q-td>
                </q-tr>
              </template> -->
              <template #header="props">
                <q-tr :props="props" class="bg-primary text-white">
                  <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                </q-tr>
              </template>
              <template #body="props">
                <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                  <q-td>{{ props.row.expenseNumber }}</q-td>
                  <q-td>{{ props.row.createdOnUtc }}</q-td>
                  <q-td class="text-right">{{ props.row.amount }}</q-td>
                  <q-td>{{ props.row.expenseStatus.dropDownValue }}</q-td>
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
import { ref, onMounted } from "vue";
import bankAccountService from "modules/finance-bank-account/financeBankAccount.service"; // Import the appropriate service
import _ from "lodash";

const transactions = ref([]);
const rows = ref([]);

// Define the table columns for transactions (if used)
const columns = [
  { name: "expenseNumber", label: "Transaction ID", align: "left", field: "expenseNumber" },
  { name: "createdOnUtc", label: "Date", align: "left", field: "createdOnUtc" },
  { name: "amount", label: "Amount", align: "left", field: "amount" },
  { name: "expenseStatus.dropDownValue", label: "Status", align: "expenseStatus.dropDownValue", field: "status" }
];

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// Define model values
const model = ref({
  accountTypeDropDown: {
    dropDownValue: ""
  }
});

// Function to fetch bank account details by ID
const getBankAccountDetails = () => {
  bankAccountService.getBankAccountById(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.accountStatus = resp.isActive ? "Active" : "Inactive";
    rows.value = resp.expenseList.map(item => ({
      ...item
    }));

    // Handle transactions if they are part of the response or fetch separately
    transactions.value = resp.transactions || [];
  }).catch((error) => {
    console.error("Error fetching account details:", error);
  });
};

// Fetch bank account details when the component is mounted
onMounted(() => {
  if (props.id) getBankAccountDetails();
});

</script>
