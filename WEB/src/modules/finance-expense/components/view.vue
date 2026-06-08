<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent full-height position="right" @hide="onDialogHide">
    <q-card
      class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 1000px; height: 100% !important;max-width: 100vw;"
    >
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h3 text-white">View Expense</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense :disable="processing" />
      </q-card-section>
      <q-separator />

      <!-- Content Section -->
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <!-- Expense Information -->
          <fieldset>
            <legend>Expense Info</legend>
            <div class="row">
              <div class="col-12 col-sm-4 col-md-4 q-pb-sm">
                <div class="q-mb-xs">Expense No</div>
                <div class="text-black">
                  {{ model.expenseNumber }}
                </div>
              </div>
              <div class="col-12 col-sm-4 col-md-4 q-pb-sm">
                <div class="q-mb-xs">Date</div>
                <div class="text-black">
                  {{ model.expenseDate }}
                </div>
              </div>
              <div class="col-12 col-sm-4 col-md-4 q-pb-sm">
                <div class="q-mb-xs">Vendor</div>
                <div class="text-black">
                  {{ model.payee }}
                </div>
              </div>
            </div>
            <div class="row">
              <div class="col-12 col-sm-4 col-md-4 q-pb-sm">
                <div class="q-mb-xs">Vendor Payment Method</div>
                <div class="text-black">
                  {{ model.paymentType }}
                </div>
              </div>
              <div class="col-12 col-sm-4 col-md-4 q-pb-sm hidden">
                <div class="q-mb-xs">Ref No.</div>
                <div class="text-black">
                  {{ model.refNo }}
                </div>
              </div>
              <div class="col-12 col-sm-4 col-md-4 q-pb-sm">
                <div class="q-mb-xs">Status</div>
                <div class="text-black">
                  {{ model.status }}
                </div>
              </div>
              <div class="col-12 col-sm-4 col-md-4 ">
                <div class="q-mb-xs">Location</div>
                <div class="text-black">
                  {{ model.location }}
                </div>
              </div>
            </div>
            <div class="row">
              <div class="col-12">
                <div class="q-mb-xs">Memo</div>
                <div class="text-black RichTextEditor">
                  <p v-html="model.memo" />
                </div>
              </div>
              <div v-if="model.virtualpath" class="col-12 col-sm-4 col-md-4 ">
                <div>
                  <div class="q-mb-xs">Attachment</div>
                  <div :class="isImageFile(model.virtualpath) ? 'row justify-center' : 'q-mt-sm'">
                    <!-- Conditional Rendering based on File Type -->
                    <img v-if="isImageFile(model.virtualpath)" :src="model.virtualpath" alt="Attachment Preview" style="width: 30%;">
                    <a v-else :href="model.virtualpath" target="_blank">
                      {{ extractFileName(model.seoFilename) }}
                    </a>
                  </div>
                </div>
              </div>
            </div>
            <div class="row">
              <div class="col-12 col-sm-4 col-md-4 q-pb-sm">
                <div class="q-mb-xs">Is Reimbursement</div>
                <div class="text-black">
                  {{ model.isReImbursement }}
                </div>
              </div>
              <div class="col-12 col-sm-4 col-md-4 q-pb-sm">
                <div class="q-mb-xs">Recurring Start Date</div>
                <div class="text-black">
                  {{ model.recurringStartDate }}
                </div>
              </div>
              <div class="col-12 col-sm-4 col-md-4 q-pb-sm">
                <div class="q-mb-xs">Recurring End Date</div>
                <div class="text-black">
                  {{ model.recurringEndDate }}
                </div>
              </div>
            </div>
            <div class="row">
              <div class="col-12 col-sm-4 col-md-4 q-pb-sm">
                <div class="q-mb-xs">Created By</div>
                <div class="text-black">
                  {{ model.createdBy }}
                </div>
              </div>
              <div class="col-12 col-sm-4 col-md-4 q-pb-sm">
                <div class="q-mb-xs">Created Date</div>
                <div class="text-black">
                  {{ model.createdOnUtc }}
                </div>
              </div>
              <div class="col-12 col-sm-4 col-md-4 q-pb-sm">
                <div class="q-mb-xs">Updated By</div>
                <div class="text-black">
                  {{ model.updatedBy }}
                </div>
              </div>
            </div>
            <div class="row">
              <div class="col-12 col-sm-4 col-md-4 q-pb-sm">
                <div class="q-mb-xs">Updated Date</div>
                <div class="text-black">
                  {{ model.updatedOnUtc }}
                </div>
              </div>
              <div class="col-12 col-sm-4 col-md-4  q-pb-sm">
                <div class="q-mb-xs">Approver Note</div>
                <div class="text-black RichTextEditor" v-html="model.postApproverNote" />
              </div>
              <div class="col-12 col-sm-4 col-md-4  q-pb-sm">
                <div class="q-mb-xs">Pre Approver Note</div>
                <div class="text-black RichTextEditor" v-html="model.preApproverNote" />
              </div>
            </div>
            <div class="row">
              <div class="col-12 col-sm-4 col-md-4  q-pb-sm">
                <div class="q-mb-xs">Paid By Note</div>
                <div class="text-black RichTextEditor" v-html="model.paidByNote" />
              </div>
            </div>
          </fieldset>
          <!-- Expenses Category List -->
          <fieldset>
            <legend>Expense Lines</legend>
            <q-table ref="tableRef" v-model:pagination="pagination" flat bordered separator="cell" :rows="expenses" :columns="columns" row-key="expenseCategory" class="no-shadow" :loading="loading" :rows-per-page-options="[20, 50, 100, 200, 500]">
              <template #header="props">
                <q-tr :props="props" class="bg-primary text-white">
                  <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                </q-tr>
              </template>
              <template #body="props">
                <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                  <q-td>{{ props.row.expenseCategory }}</q-td>
                  <q-td>{{ props.row.subcategory }}</q-td>
                  <q-td class="text-left" style="width: 300px; max-width: 300px; white-space: normal; overflow-wrap: break-word;">{{ props.row.description }}</q-td>
                  <q-td class="text-right">{{ props.row.quantity }}</q-td>
                  <q-td class="text-right">{{ props.row.unitPrice }}</q-td>
                  <q-td class="text-right">{{ props.row.amount }}</q-td>
                </q-tr>
              </template>
              <template #bottom-row>
                <q-tr class="bg-grey-2 text-weight-bold">
                  <q-td colspan="5" class="text-right">Total Amount:</q-td>
                  <q-td class="text-right">{{ totalAmountForPage }}</q-td>
                </q-tr>
              </template>
            </q-table>
          </fieldset>
          <fieldset v-if="filesrows.length > 0" class="q-mb-lg">
            <legend>Expense Files</legend>
            <q-table
              ref="tableRef" v-model:pagination="filepagination" bordered class="no-shadow" :loading="loading" :rows="filesrows" :columns="fileColumns" row-key="id" separator="cell"
              binary-state-sort :rows-per-page-options="[20, 50, 100, 200, 500]"
            >
              <template #header="props">
                <q-tr :props="props" class="bg-primary text-white">
                  <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                  <q-th auto-width class="text-center">Actions</q-th>
                </q-tr>
              </template>
              <template #body="props">
                <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                  <q-td>{{ extractFileName(props.row.file.seoFilename) }}</q-td>
                  <q-td>{{ props.row.createdBy.person.firstName + " " + props.row.createdBy.person.lastName }}</q-td>
                  <q-td>{{ props.row.createdOnUtc.replaceAll("-", "/") }}</q-td>
                  <q-td style="width: 5%;" class="text-center actions">
                    <q-btn icon="o_visibility" size="sm" class="q-pr-xs" flat @click="viewFile(props.row.file.virtualPath)" />
                    <q-btn icon="o_download" size="sm" class="q-pl-xs" flat @click="downloadFile(props.row.file.virtualPath)" />
                  </q-td>
                </q-tr>
              </template>
            </q-table>
          </fieldset>
        </div>
      </div>
      <q-card-actions v-if="isShowAction" align="center" class="q-gutter-sm justify-center">
        <q-btn
          v-close-popup
          label="Close"
          color="grey-4"
          class="text-grey-9 actionBtn"
          no-caps
          :disable="processing"
        />
        <q-btn
          v-if="(roles.includes('finance-approver') || roles.includes('finance-preapprove') || role) && model.status == 'Request For Cancellation'"
          label="Cancel Request"
          color="grey-4"
          class="text-grey-9 actionBtn"
          no-caps
          :disabled="processing"
          :loading="processing && activeButton === 'Cancelled'"
          @click="onForwardToApprover('Cancelled')"
        />
        <q-btn
          v-if="model.status !== 'Approved' && model.status != 'Request For Cancellation'"
          label="Decline"
          color="grey-4"
          class="text-grey-9 actionBtn"
          no-caps
          :disabled="processing"
          :loading="processing && activeButton === 'Declined'"
          @click="onForwardToApprover('Declined')"
        />
        <q-btn
          v-if="model.status === 'Submitted' && (roles.includes('finance-preapprove') || role)"
          label="Send To Approver"
          color="primary"
          :disabled="processing"
          class="actionBtn" no-caps
          :loading="processing && activeButton === 'Pre-Approved'"
          @click="onForwardToApprover('Pre-Approved')"
        />
        <q-btn
          v-else-if="model.status === 'Pre-Approved' && (roles.includes('finance-approver') || role)"
          label="Approve"
          color="primary"
          :disabled="processing"
          class="actionBtn" no-caps
          :loading="processing && activeButton === 'Approved'"
          @click="onForwardToApprover('Approved')"
        />
        <q-btn
          v-if="model.status === 'Approved' && (roles.includes('finance-paidby') || role)"
          label="Update As Paid"
          color="primary"
          :disabled="processing"
          class="actionBtn" no-caps
          :loading="processing && activeButton === 'Paid'"
          @click="onForwardToApprover('Paid')"
        />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { ref, onMounted, computed } from "vue";
import { useDialogPluginComponent, useQuasar } from "quasar";
import { notifySuccess } from "assets/utils";
import { useAuthStore } from "stores/auth";

import ExpenseService from "modules/finance-expense/financeExpense.service";
import ApproverNoteDialog from "../components/approveNote.vue";

const props = defineProps({
  id: {
    type: String,
    default: null
  },
  isShowActions: {
    type: Boolean,
    default: false
  }
});

const authStore = useAuthStore();
const user = authStore.user;
const roles = user?.roles || [];
const adminRoles = ["admin", "site-super-admin", "system-super-admin"];
const role = user?.roles?.some(r => adminRoles.includes(r)) ? "admin" : "";

// Reactive variables
const $q = useQuasar();
const expenses = ref([]); // Expense categories list
const loading = ref(true);
// const baseURL = process.env.API_BASE_URL;
const activeButton = ref("");
const processing = ref(false);

defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK } = useDialogPluginComponent();

const expenseModel = ref({});
const model = ref({});
const pagination = ref({ sortBy: "", descending: false, rowsPerPage: 20, page: 1 });
const columns = [
  { name: "expenseCategory", label: "Category", align: "left", field: "expenseCategory", sortable: true },
  { name: "subcategory", label: "Sub category", align: "left", field: "subcategory", sortable: true },
  { name: "description", label: "Description", align: "left", field: "description", sortable: true },
  { name: "quantity", label: "Quantity", align: "right", field: "quantity", sortable: true },
  { name: "unitPrice", label: "Unit Price", align: "right", field: "unitPrice", sortable: true },
  { name: "amount", label: "Amount", align: "right", field: "amount", sortable: true }
];

const filesrows = ref([]);
const filepagination = ref({ sortBy: "", descending: true, rowsPerPage: 20, page: 1 });
const fileColumns = ref([
  { name: "virtualPath", label: "Name", field: "file.virtualPath", align: "left" },
  { name: "createdBy.person.firstName", label: "Created By", field: "createdBy.person.firstName", align: "left" },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "left" }
]);

const isShowAction = props.isShowActions;

function isImageFile (filePath) {
  const imageExtensions = ["jpg", "jpeg", "png", "gif"];
  const fileExtension = filePath.split(".").pop().toLowerCase();
  return imageExtensions.includes(fileExtension);
}

function extractFileName (path) {
  return path ? path.split("/").pop() : "Unknown File";
}

async function getExpenseById () {
  try {
    loading.value = true;
    const data = await ExpenseService.getExpenseById(props.id);
    expenseModel.value = data;
    model.value.location = data.location.dropDownValue;
    model.value.expenseDate = data.expenseDate.replace(/-/g, "/");
    model.value.expenseNumber = data.expenseNumber;
    model.value.payee = data.expenseVendors.vendorName;
    model.value.paymentType = data.expenseVendorBankAccounts.paymentType.dropDownValue;
    model.value.refNo = data.ref_no;
    model.value.virtualpath = data.picture.virtualPath || "";
    model.value.status = data.expenseStatus.dropDownValue;
    model.value.attachment = data.attachment;
    model.value.memo = data.memo;
    model.value.description = data.description;
    model.value.createdBy = data.createdBy.person.fullName;
    model.value.updatedBy = data.updatedBy.person.fullName;
    model.value.createdOnUtc = data.createdOnUtc;
    model.value.updatedOnUtc = data.updatedOnUtc;
    model.value.amount = data.amount;
    model.value.recurringStartDate = data.recurringStartDate;
    model.value.recurringEndDate = data.recurringEndDate;
    model.value.postApproverNote = data.postApproverNote;
    model.value.preApproverNote = data.preApproverNote;
    model.value.paidByNote = data.paidByNote;
    model.value.isReImbursement = data.isReImbursement ? "Yes" : "No";
    filesrows.value = data.expenseFilesList.map(item => ({
      ...item
    }));
    // Map expenses (categories list) to the required structure
    expenses.value = (data.expenseLines || []).map((expense) => ({
      id: expense.id,
      expenseCategory: expense.category.type,
      subcategory: expense.expenseCategorySubcategory.dropDownValue,
      description: expense.description,
      amount: expense.amount,
      quantity: expense.quantity,
      unitPrice: expense.unitPrice
    }));
  } catch (error) {
    console.error("Error fetching expense details:", error);
  } finally {
    loading.value = false;
  }
}

function downloadFile (file) {
  const link = document.createElement("a");
  link.href = file;
  link.download = file.split("/").pop();
  link.click();
}

function viewFile (file) {
  const fileUrl = new URL(file).href;
  const fileExtension = fileUrl.split(".").pop().toLowerCase();
  const supportedFormats = ["pdf", "docx", "xlsx", "pptx"];
  const imageFormats = ["jpg", "jpeg", "png", "gif", "svg"];

  let viewerUrl = fileUrl;
  // Use Google Docs Viewer for Documents
  if (supportedFormats.includes(fileExtension)) {
    viewerUrl = `https://docs.google.com/gview?url=${encodeURIComponent(fileUrl)}&embedded=true`;
    // console.log("googleDocsViewer", viewerUrl);
  }

  // Open new window
  const newWindow = window.open("", "_blank");

  // Check if the popup is blocked
  if (!newWindow) {
    alert("Popup blocked! Please allow popups for this site.");
    return;
  }

  // Delay to avoid null reference issues
  setTimeout(() => {
    newWindow.document.write(`
<html>
<head>
<title>${file.split("/").pop()}</title>
<style>
              * { margin: 0; padding: 0; box-sizing: border-box; }
              body, html { width: 100vw; height: 100vh; display: flex; align-items: center; justify-content: center; background-color: #f4f4f4; overflow: hidden; }
              .top-right {
                position: fixed;
                top: 10px;
                right: 10px;
                background: #007bff;
                color: white;
                padding: 10px 15px;
                border-radius: 5px;
                font-size: 16px;
                text-decoration: none;
                z-index: 10;
              }
              .top-right:hover {
                background: #0056b3;
              }

              iframe, img {
                width: 100%;
                height: 100%;
                border: none;
                display: block;
                object-fit: contain; /* Ensures images fit properly */
              }

              /* Responsive Fixes */
              @media (max-width: 768px) {
                .top-right {
                  top: 5px;
                  right: 5px;
                  padding: 8px 12px;
                  font-size: 14px;
                }
              }
</style>
</head>
<body>
<a class="top-right" href="${fileUrl}" download>Download</a>
            ${
  imageFormats.includes(fileExtension)
    ? `<img src="${fileUrl}" alt="Image Preview">` // Show image directly
    : `<iframe src="${viewerUrl}"></iframe>` // Show document using iframe
}
</body>
</html>
        `);
  }, 100);
}

const onForwardToApprover = (approver) => {
  activeButton.value = approver;
  expenseModel.value.approver = approver;
  $q.dialog({
    component: ApproverNoteDialog,
    componentProps: {
      title: "<span class=\"text-primary\"><b>Confirm<b/></span>",
      message: approver === "Pre-Approved" ? "Are you sure you want forward to approver ?"
        : (approver === "Approved" ? "Are you sure you want to approve ?"
          : approver === "Cancelled" ? "Are you sure you want to cancel ?"
            : approver === "Paid" ? "Are you sure you have paid this expense ?" : "Are you sure you want to decline ?"),
      approver
    }
  }).onOk((note) => {
    processing.value = true;
    if (approver === "Approved") {
      expenseModel.value.postApproverNote = note;
    } else if (approver === "Pre-Approved") {
      expenseModel.value.preApproverNote = note;
    } else if (approver === "Paid") {
      expenseModel.value.paidByNote = note;
    } else if (approver === "Cancelled") {
      expenseModel.value.preApproverNote = note;
    } else if (approver === "Declined") {
      if (roles.includes("finance-approver")) {
        expenseModel.value.postApproverNote = note;
      } else if (roles.includes("finance-preapprove")) {
        expenseModel.value.preApproverNote = note;
      }
    }
    processing.value = true;
    ExpenseService.forwardExpenseToApprovers(expenseModel.value)
      .then(() => {
        notifySuccess({
          message: approver === "Pre-Approved" ? "Forwarded Successfully!" : (approver === "Approved" ? "Approved Successfully!" : (approver === "Paid" ? "Update Status Successfully!" : "Declined Successfully!"))
        });
        onDialogOK();
      })
      .catch(() => {
        $q.notify({
          type: "negative",
          message: "Failed to update expense status."
        });
      })
      .finally(() => {
        setTimeout(() => {
          processing.value = false;
          activeButton.value = "";
        }, 1500);
      });
  });
};

const paginatedRows = computed(() => {
  if (pagination.value.rowsPerPage === 0) {
    // If rowsPerPage is 0, show all rows
    return expenses.value;
  }
  const start = (pagination.value.page - 1) * pagination.value.rowsPerPage;
  const end = pagination.value.page * pagination.value.rowsPerPage;
  return expenses.value.slice(start, end);
});

const totalAmountForPage = computed(() => {
  return paginatedRows.value
    .reduce((sum, row) => sum + (parseFloat(row.amount) || 0), 0);
});

onMounted(() => {
  if (props.id) getExpenseById();
});

</script>
