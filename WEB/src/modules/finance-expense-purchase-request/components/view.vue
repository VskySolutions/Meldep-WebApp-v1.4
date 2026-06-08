<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent full-height position="right" @hide="onDialogHide">
    <q-card
      class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 1000px; height: 100% !important;max-width: 100vw;"
    >
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h3 text-white">View Purchase Expense</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense :disable="processing"/>
      </q-card-section>
      <q-separator />

      <!-- Content Section -->
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <!-- Expense Information -->
          <fieldset>
            <legend>Purchase Expense Info</legend>
            <div class="row">
              <div class="col-12 col-sm-4 col-md-4  q-pb-sm">
                <div class="q-mb-xs">Request Date</div>
                <div class="text-black">
                  {{ model.requestDate }}
                </div>
              </div>
              <div class="col-12 col-sm-4 col-md-4  q-pb-sm">
                <div class="q-mb-xs">ReferenceId</div>
                <div class="text-black">
                  {{ model.referenceId }}
                </div>
              </div>
              <div class="col-12 col-sm-4 col-md-4  q-pb-sm">
                <div class="q-mb-xs">Vendor</div>
                <div class="text-black">
                  {{ model.vendor }}
                </div>
              </div>
            </div>
            <div class="row">
              <div class="col-12 col-sm-4 col-md-4  q-pb-sm">
                <div class="q-mb-xs">Item Name</div>
                <div class="text-black">
                  {{ model.itemName }}
                </div>
              </div>
              <div class="col-12 col-sm-4 col-md-4  q-pb-sm">
                <div class="q-mb-xs">Item Category</div>
                <div class="text-black">
                  {{ model.itemCategory }}
                </div>
              </div>
              <div class="col-12 col-sm-4 col-md-4  q-pb-sm">
                <div class="q-mb-xs">Item Sub Category</div>
                <div class="text-black">
                  {{ model.itemSubCategory }}
                </div>
              </div>
            </div>
            <div class="row">
              <div class="col-12 col-sm-4 col-md-4  q-pb-sm">
                <div class="q-mb-xs">Requested By</div>
                <div class="text-black">
                  {{ model.requestedEmployee }}
                </div>
              </div>
              <div class="col-12 col-sm-4 col-md-4  q-pb-sm">
                <div class="q-mb-xs">Purchase By</div>
                <div class="text-black">
                  {{ model.purchaserEmployee }}
                </div>
              </div>
              <div class="col-12 col-sm-4 col-md-4  q-pb-sm">
                <div class="q-mb-xs">Quantity</div>
                <div class="text-black">
                  {{ model.quantity }}
                </div>
              </div>
            </div>
            <div class="row">
              <div class="col-12 col-sm-4 col-md-4  q-pb-sm">
                <div class="q-mb-xs">Estimated Rate</div>
                <div class="text-black">
                  {{ model.estimatedRate }}
                </div>
              </div>
              <div class="col-12 col-sm-4 col-md-4  q-pb-sm">
                <div class="q-mb-xs">Discount</div>
                <div class="text-black">
                  {{ model.discount }}
                </div>
              </div>
              <div class="col-12 col-sm-4 col-md-4  q-pb-sm">
                <div class="q-mb-xs">Estimated Amount</div>
                <div class="text-black">
                  {{ model.estimatedAmount }}
                </div>
              </div>
            </div>
            <div class="row">
              <div class="col-12 col-sm-4 col-md-4  q-pb-sm">
                <div class="q-mb-xs">Description</div>
                <div class="text-black RichTextEditor" v-html="model.description" />
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
          <fieldset class="q-mb-lg">
            <legend>Purchase Expense Files</legend>
            <q-table
              ref="tableRef"
              v-model:pagination="filePagination"
              bordered class="no-shadow"
              :loading="loading"
              :rows="filesRows"
              :columns="fileColumns"
              row-key="id"
              separator="cell"
              binary-state-sort
              :rows-per-page-options="[20, 50, 100, 200, 500]"
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
      <q-card-actions v-if="isShowActions" align="center" class="q-gutter-sm justify-center">
        <q-btn
          v-close-popup
          label="Close"
          color="grey-4"
          class="text-grey-9 actionBtn"
          :disable="processing"
          no-caps
        />
        <q-btn
          v-if="(roles.includes('finance-approver') || roles.includes('finance-preapprove') || role) && model.purchaseExpenseStatus == 'Request For Cancellation'"
          label="Cancel Request"
          color="grey-4"
          class="text-grey-9 actionBtn" no-caps
          :disabled="processing"
          :loading="processing && activeButton === 'Cancelled'"
          @click="onForwardToApprover('Cancelled')"
        />
        <q-btn
          v-if="model.purchaseExpenseStatus !== 'Approved' && model.purchaseExpenseStatus != 'Request For Cancellation'"
          label="Decline"
          color="grey-4"
          class="text-grey-9 actionBtn" no-caps
          :disabled="processing"
          :loading="processing && activeButton === 'Declined'"
          @click="onForwardToApprover('Declined')"
        />
        <q-btn
          v-if="model.purchaseExpenseStatus === 'Submitted' && (roles.includes('finance-preapprove') || role)"
          label="Send To Approver"
          color="primary"
          class="actionBtn" no-caps
          :disabled="processing"
          :loading="processing && activeButton === 'Pre-Approved'"
          @click="onForwardToApprover('Pre-Approved')"
        />
        <q-btn
          v-else-if="model.purchaseExpenseStatus === 'Pre-Approved' && (roles.includes('finance-approver') || role)"
          label="Approve"
          color="primary"
          class="actionBtn" no-caps
          :disabled="processing"
          :loading="processing && activeButton === 'Approved'"
          @click="onForwardToApprover('Approved')"
        />
        <q-btn
          v-if="model.purchaseExpenseStatus === 'Approved' && (roles.includes('finance-paidby') || role)"
          label="Update As Paid"
          color="primary"
          class="actionBtn" no-caps
          :disabled="processing"
          :loading="processing && activeButton === 'Paid'"
          @click="onForwardToApprover('Paid')"
        />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useDialogPluginComponent, useQuasar } from "quasar";
import { notifySuccess } from "assets/utils";
import { useAuthStore } from "stores/auth";

import purchaseExpensesService from "../financeExpensePurchaseRequest.service";
import ApproverNoteDialog from "modules/finance-expense/components/approveNote.vue";

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
// const baseURL = process.env.API_BASE_URL;
// Reactive variables
const $q = useQuasar();
const loading = ref(true);
const processing = ref(false);
const activeButton = ref("");

defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK } = useDialogPluginComponent();

const purchaseExpenseModel = ref({});
const model = ref({});

const filesRows = ref([]);
const filePagination = ref({ sortBy: "", descending: true, rowsPerPage: 20, page: 1 });
const fileColumns = ref([
  { name: "virtualPath", label: "File Name", field: "file.virtualPath", align: "left" },
  { name: "createdBy.person.firstName", label: "Created By", field: "createdBy.person.firstName", align: "left" },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "left" }
]);

async function getPurchaseExpenseDetailsById () {
  try {
    loading.value = true;
    const data = await purchaseExpensesService.getPurchaseExpenseDetailsById(props.id);
    purchaseExpenseModel.value = data;
    model.value.id = data.id;
    model.value.referenceId = data.referenceId;
    model.value.requestDate = data.requestDate;
    model.value.vendor = data.expenseVendors.vendorName;
    model.value.itemName = data.itemName;
    model.value.requestedEmployee = data.requestedEmployee.person.fullName;
    model.value.purchaserEmployee = data.purchaserEmployee.person.fullName;
    model.value.quantity = data.quantity;
    model.value.estimatedRate = data.estimatedRate;
    model.value.discount = data.discount;
    model.value.estimatedAmount = data.estimatedAmount;
    model.value.description = data.description;
    model.value.itemCategory = data.itemCategory.type;
    model.value.itemSubCategory = data.itemSubCategory.dropDownValue;
    model.value.purchaseExpenseStatus = data.purchaseRequestStatus.dropDownValue;
    model.value.postApproverNote = data.postApproverNote;
    model.value.preApproverNote = data.preApproverNote;
    model.value.paidByNote = data.paidByNote;
    filesRows.value = data.expensePurchaseRequestFileList || [];
  } catch (error) {
    console.error("Error fetching expense details:", error);
  } finally {
    loading.value = false;
  }
}

const onForwardToApprover = (approver) => {
  activeButton.value = approver;
  purchaseExpenseModel.value.approver = approver;
  $q.dialog({
    component: ApproverNoteDialog,
    componentProps: {
      title: "<span class=\"text-primary\"><b>Confirm</b></span>",
      message: approver === "Pre-Approved" ? "Are you sure you want forward to approver ?"
        : (approver === "Approved" ? "Are you sure you want to approve ?"
          : approver === "Cancelled" ? "Are you sure you want to cancel ?" : "Are you sure you want to decline ?"),
      approver
    }
  }).onOk((note) => {
    processing.value = true;
    if (approver === "Approved") {
      purchaseExpenseModel.value.postApproverNote = note;
    } else if (approver === "Pre-Approved") {
      purchaseExpenseModel.value.preApproverNote = note;
    } else if (approver === "Paid") {
      purchaseExpenseModel.value.paidByNote = note;
    } else if (approver === "Cancelled") {
      purchaseExpenseModel.value.preApproverNote = note;
    } else if (approver === "Declined") {
      if (roles.includes("finance-preapprove")) {
        purchaseExpenseModel.value.preApproverNote = note;
      } else if (roles.includes("finance-approver")) {
        purchaseExpenseModel.value.postApproverNote = note;
      }
    }
    processing.value = true;
    purchaseExpensesService.forwardPurchaseExpenseToApprovers(purchaseExpenseModel.value)
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

function extractFileName (path) {
  return path ? path.split("/").pop() : "Unknown File";
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
  if (supportedFormats.includes(fileExtension)) {
    viewerUrl = `https://docs.google.com/gview?url=${encodeURIComponent(fileUrl)}&embedded=true`;
  }
  const newWindow = window.open("", "_blank");
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

onMounted(() => {
  if (props.id) getPurchaseExpenseDetailsById();
});

</script>
