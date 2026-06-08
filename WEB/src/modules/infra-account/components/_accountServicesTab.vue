<template>
  <fieldset class="q-mb-lg">
    <q-table
      v-model:pagination="pagination"
      bordered
      class="no-shadow"
      :loading="loading"
      :rows="rows"
      :columns="servicesColumns"
      row-key="id"
      separator="cell"
      binary-state-sort
      :rows-per-page-options="[20,50,100,200,500]"
    >
      <template #header="props">
        <q-tr :props="props" class="bg-primary text-white">
          <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
        </q-tr>
      </template>

      <template #body="props">
        <q-tr :props="props">
          <q-td style="width: 8%">{{ props.row.itemType.dropDownValue }}</q-td>
          <q-td style="width: 8%">{{ props.row.ownerShipType.dropDownValue }}</q-td>
          <q-td style="width: 10%">{{ props.row.name }}</q-td>
          <q-td style="width: 10%"><div class="ellipsis-cell">
            <a
              :href="props.row.url"
              target="_blank"
              class="link-text"
            >
              {{ props.row.url }}
            </a>
          </div>
          </q-td>
          <q-td style="width: 8%">{{ props.row.startDate }}</q-td>
          <q-td style="width: 8%">{{ props.row.endDate }}</q-td>
          <q-td style="width: 8%">{{ props.row.paymentTerm.dropDownValue }}</q-td>
          <q-td v-if="isShowFlag" align="right" style="width: 10%">
            ${{ props.row.actualPriceInDollar }}
            <q-icon
              v-if="Number(props.row.actualPriceInDollar) !== Number(props.row.priceInDollar)"
              name="o_info"
              size="16px"
              color="grey-7"
              class="q-ml-xs cursor-pointer"
            >
              <q-tooltip>
                Actual Price: ${{ props.row.priceInDollar }}
              </q-tooltip>
            </q-icon>
          </q-td>
          <q-td v-else align="right" style="width: 10%">
            ${{ props.row.priceInDollar }}
          </q-td>
          <q-td style="width: 8%">{{ props.row.walletType.dropDownValue }}</q-td>
          <q-td style="width: 8%">{{ props.row.walletNumber }}</q-td>
          <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 14%;">
            <p v-html="props.row.instructions || '-'" />
          </q-td>
        </q-tr>
      </template>
      <template #bottom-row>
        <q-tr v-if="props.rows.length" class="bg-grey-2 text-black">
          <q-td colspan="7" class="text-right text-weight-bold">
            Total Price:
          </q-td>
          <q-td class="text-right text-weight-bold">
            ${{ totalPrice.toFixed(2) }}
          </q-td>
          <q-td />
          <q-td />
          <q-td />
        </q-tr>
      </template>
    </q-table>
  </fieldset>
</template>

<script setup>
import { ref, computed } from "vue";

const props = defineProps({
  rows: {
    type: Array,
    default: () => []
  },
  loading: Boolean,
  isShow: {
    type: Boolean,
    default: true
  }
});
const isShowFlag = computed(() => props.isShow === true);

const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const servicesColumns = ref([
  { name: "itemTypeId", label: "Item Type", field: "itemTypeId", align: "left", sortable: true },
  { name: "ownerShipTypeId", label: "Ownership Type", field: "ownerShipTypeId", align: "left", sortable: true },
  { name: "name", label: "Name", field: "name", align: "left", sortable: true },
  { name: "url", label: "URL", field: "url", align: "left", sortable: true },
  { name: "startDateStr", label: "Start Date", field: "startDateStr", align: "left", sortable: true },
  { name: "endDate", label: "End Date", field: "endDate", align: "left", sortable: true },
  { name: "paymentTermId", label: "Payment Term", field: "paymentTermId", align: "left", sortable: true },
  { name: "priceInDollar", label: "Price", field: "priceInDollar", align: "right", sortable: true },
  { name: "walletTypeId", label: "Wallet Type", field: "walletTypeId", align: "left", sortable: true },
  { name: "walletNumber", label: "Wallet Number", field: "walletNumber", align: "left", sortable: true },
  { name: "instructions", label: "Instructions", field: "instructions", align: "left", sortable: true }
]);

const totalPrice = computed(() => {
  return props.rows.reduce((sum, row) => {
    let price = 0;

    if (isShowFlag.value) {
      price = parseFloat(row.actualPriceInDollar) || 0;
    } else {
      price = parseFloat(row.priceInDollar) || 0;
    }
    return sum + price;
  }, 0);
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
