<template>
  <q-page padding>
    <!-- ===================== Header / Toolbar ===================== -->
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-sm-5">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Infrastructure" />
              <q-breadcrumbs-el label="Dashboard & Financial Insights" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-sm-7">
            <div class="row items-center justify-end no-wrap q-gutter-xs">
              <q-btn icon="o_filter_alt" outline no-caps class="text-primary btnRounded" label="Filters" @click="showFilter = !showFilter">
                <q-badge v-if="activeFilterCount > 0" color="green" floating>{{ activeFilterCount }}</q-badge>
                <q-menu v-model="showFilter" anchor="bottom right" self="top right" persistent no-parent-event style="width: 460px;" @click-outside="showFilter = false">
                  <q-card class="q-pa-sm">
                    <div class="text-subtitle2 text-primary q-mb-xs">Dashboard Filters</div>
                    <multiSelectDropdown
                      v-model="filter.providerIds"
                      label="Provider"
                      :options="providerOptions.list.value"
                      :filter="providerOptions.filter"
                    />
                    <multiSelectDropdown
                      v-model="filter.projectIds"
                      label="Project"
                      :options="projectOptions.list.value"
                      :filter="projectOptions.filter"
                    />
                    <multiSelectDropdown
                      v-model="filter.itemTypeIds"
                      label="Service Type"
                      :options="itemTypeOptions.list.value"
                      :filter="itemTypeOptions.filter"
                    />
                    <multiSelectDropdown
                      v-model="filter.ownershipTypeIds"
                      label="Ownership Type"
                      :options="ownershipOptions.list.value"
                      :filter="ownershipOptions.filter"
                    />
                    <multiSelectDropdown
                      v-model="filter.paymentTermIds"
                      label="Payment Term"
                      :options="paymentTermOptions.list.value"
                      :filter="paymentTermOptions.filter"
                    />
                    <div class="row q-col-gutter-sm q-mt-xs">
                      <div class="col-6">
                        <q-input v-model="filter.fromDate" dense outlined stack-label type="date" label="Trend From" />
                      </div>
                      <div class="col-6">
                        <q-input v-model="filter.toDate" dense outlined stack-label type="date" label="Trend To" />
                      </div>
                    </div>
                    <div class="row justify-end q-gutter-sm q-mt-md q-mb-xs">
                      <q-btn outline color="primary" label="Apply" class="btnRounded" no-caps @click="onApply" />
                      <q-btn outline color="grey-4" label="Clear" class="text-grey-9 btnRounded" no-caps @click="onClearFilters" />
                      <q-btn outline color="negative" label="Close" class="btnRounded" no-caps @click="showFilter = false" />
                    </div>
                  </q-card>
                </q-menu>
              </q-btn>
              <q-btn icon="o_refresh" outline no-caps class="text-primary btnRounded" @click="refreshAll">
                <q-tooltip>Refresh</q-tooltip>
              </q-btn>
              <q-btn
                icon="o_arrow_back"
                outline
                label="Back to Accounts"
                no-caps
                class="text-primary btnRounded"
                @click="$router.push('/infra-account')"
              />
            </div>
          </div>
        </div>
      </q-card-section>
      <q-linear-progress v-if="loading" indeterminate color="primary" />
    </q-card>

    <!-- ===================== Executive Summary ===================== -->
    <div class="row items-center q-mb-sm q-mt-md">
      <q-icon name="o_account_balance_wallet" color="primary" size="22px" class="q-mr-sm" />
      <span class="text-h6 text-primary text-weight-bold">Executive Cost Summary</span>
    </div>
    <div class="row q-col-gutter-md">
      <div class="col-12 col-sm-6 col-md-4">
        <kpiCard label="Monthly Equivalent Cost" :value="money0(summary?.monthlyEquivalentTotal)"
          sub="Active recurring services" icon="o_calendar_month" accent="#1b75ab" />
      </div>
      <div class="col-12 col-sm-6 col-md-4">
        <kpiCard label="Annualized (Yearly) Cost" :value="money0(summary?.annualizedTotal)"
          :sub="yearlyMonthlySub" icon="o_trending_up" accent="#20b2aa" />
      </div>
      <div class="col-12 col-sm-6 col-md-4">
        <kpiCard label="Year-to-Date Cost (YTD)" :value="money0(summary?.ytdTotal)"
          sub="Cost spent to date" icon="o_paid" accent="#2e7d32" />
      </div>
      <div class="col-12 col-sm-6 col-md-4">
        <kpiCard label="One-Time Cost" :value="money0(summary?.oneTimeTotal)"
          :sub="`${summary?.oneTimeServiceCount ?? 0} one-time service(s)`" icon="o_receipt_long" accent="#e6842e" />
      </div>
      <div class="col-12 col-sm-6 col-md-4">
        <kpiCard label="Services Under Management" :value="summary?.serviceCount ?? '—'"
          :sub="`${summary?.accountCount ?? 0} account(s) • ${summary?.recurringServiceCount ?? 0} recurring`"
          icon="o_dns" accent="#8e6bb0" />
      </div>
    </div>

    <!-- ===================== Cost Trend ===================== -->
    <div class="row items-center q-mb-sm q-mt-lg">
      <q-icon name="o_show_chart" color="primary" size="22px" class="q-mr-sm" />
      <span class="text-h6 text-primary text-weight-bold">Cost Trend</span>
    </div>
    <div class="row q-col-gutter-md">
      <div class="col-12 col-md-8">
        <chartPanel
          title="Monthly Recurring Cost Trend"
          :subtitle="trendSubtitle"
          type="area"
          series-name="Monthly Equivalent"
          :series="history?.series || []"
          :labels="history?.labels || []"
          :height="300"
          :loading="loading"
        />
      </div>
      <div class="col-12 col-md-4">
        <chartPanel
          title="YTD vs Yearly Cost"
          :subtitle="ytdYearlySubtitle"
          type="bar"
          horizontal
          :series="ytdVsYearlySeries"
          :labels="['Year-to-Date', 'Annualized (Yearly)']"
          :colors="['#2e7d32', '#20b2aa']"
          :height="300"
          :loading="loading"
        />
      </div>
    </div>

    <!-- ===================== Cost Breakdowns ===================== -->
    <div class="row items-center q-mb-sm q-mt-lg">
      <q-icon name="o_donut_large" color="primary" size="22px" class="q-mr-sm" />
      <span class="text-h6 text-primary text-weight-bold">Cost Breakdowns</span>
      <span class="text-caption text-grey-6 q-ml-sm">% of the selected (filtered) total</span>
    </div>

    <!-- Yearly cost breakdowns -->
    <div class="row items-center q-mb-xs q-mt-sm">
      <q-chip dense square color="teal-1" text-color="teal-9" icon="o_trending_up">Yearly Cost</q-chip>
      <span class="text-caption text-grey-6 q-ml-xs">Hover a slice to see the monthly equivalent (yearly &divide; 12)</span>
    </div>
    <div class="row q-col-gutter-md">
      <div class="col-12 col-md-4">
        <chartPanel title="Cost by Provider" subtitle="Yearly cost" monthly-from-yearly type="donut"
          :series="breakdowns?.byProvider?.series || []" :labels="breakdowns?.byProvider?.labels || []" :loading="loading" />
      </div>
      <div class="col-12 col-md-4">
        <chartPanel title="Cost by Service Type" subtitle="Yearly cost" monthly-from-yearly type="donut"
          :series="breakdowns?.byItemType?.series || []" :labels="breakdowns?.byItemType?.labels || []" :loading="loading" />
      </div>
      <div class="col-12 col-md-4">
        <chartPanel title="Cost by Ownership Type" subtitle="Yearly cost" monthly-from-yearly type="donut"
          :series="breakdowns?.byOwnershipType?.series || []" :labels="breakdowns?.byOwnershipType?.labels || []" :loading="loading" />
      </div>
    </div>

    <!-- Till-now (YTD) cost breakdowns -->
    <div class="row items-center q-mb-xs q-mt-md">
      <q-chip dense square color="green-1" text-color="green-9" icon="o_paid">Till-Now Cost (YTD)</q-chip>
      <span class="text-caption text-grey-6 q-ml-xs">Cost spent to date, distributed by group</span>
    </div>
    <div class="row q-col-gutter-md">
      <div class="col-12 col-md-4">
        <chartPanel title="Cost by Provider" subtitle="Till-now cost" type="donut"
          :series="breakdowns?.byProvider?.seriesYtd || []" :labels="breakdowns?.byProvider?.labels || []" :loading="loading" />
      </div>
      <div class="col-12 col-md-4">
        <chartPanel title="Cost by Service Type" subtitle="Till-now cost" type="donut"
          :series="breakdowns?.byItemType?.seriesYtd || []" :labels="breakdowns?.byItemType?.labels || []" :loading="loading" />
      </div>
      <div class="col-12 col-md-4">
        <chartPanel title="Cost by Ownership Type" subtitle="Till-now cost" type="donut"
          :series="breakdowns?.byOwnershipType?.seriesYtd || []" :labels="breakdowns?.byOwnershipType?.labels || []" :loading="loading" />
      </div>
    </div>

    <!-- ===================== Price Changes ===================== -->
    <div class="row items-center q-mb-sm q-mt-lg">
      <q-icon name="o_price_change" color="primary" size="22px" class="q-mr-sm" />
      <span class="text-h6 text-primary text-weight-bold">Price Changes &amp; Cost Movement</span>
    </div>
    <q-card flat bordered>
      <q-table
        flat
        :rows="priceChanges"
        :columns="priceChangeColumns"
        row-key="serviceId"
        :pagination="{ rowsPerPage: 8 }"
        no-data-label="No price changes recorded for the selected filters"
      >
        <template #body-cell-previousPrice="props">
          <q-td :props="props" class="text-right">{{ money2(props.row.previousPrice) }}</q-td>
        </template>
        <template #body-cell-currentPrice="props">
          <q-td :props="props" class="text-right">{{ money2(props.row.currentPrice) }}</q-td>
        </template>
        <template #body-cell-absoluteChange="props">
          <q-td :props="props" class="text-right" :class="props.row.direction === 'increase' ? 'text-negative' : 'text-positive'">
            <q-icon :name="props.row.direction === 'increase' ? 'o_north_east' : 'o_south_east'" size="14px" />
            {{ money2(props.row.absoluteChange) }}
          </q-td>
        </template>
        <template #body-cell-percentageChange="props">
          <q-td :props="props" class="text-right" :class="props.row.direction === 'increase' ? 'text-negative' : 'text-positive'">
            {{ pct(props.row.percentageChange) }}
          </q-td>
        </template>
        <template #body-cell-changedOn="props">
          <q-td :props="props">{{ toDate(props.row.changedOn) }}</q-td>
        </template>
      </q-table>
    </q-card>

    <!-- ===================== Data Quality ===================== -->
    <div class="row items-center q-mb-sm q-mt-lg">
      <q-icon name="o_rule" color="primary" size="22px" class="q-mr-sm" />
      <span class="text-h6 text-primary text-weight-bold">Data Quality — Missing Billing Data</span>
    </div>
    <q-card flat bordered>
      <q-card-section class="q-py-sm">
        <div class="row q-gutter-sm items-center">
          <q-chip square color="red-1" text-color="red-9" icon="o_error_outline">
            {{ dataQuality?.totalFlagged ?? 0 }} flagged
          </q-chip>
          <q-chip v-if="(dataQuality?.missingCustomerCount ?? 0) > 0" square outline color="grey-8">Client: {{ dataQuality.missingCustomerCount }}</q-chip>
          <q-chip v-if="(dataQuality?.missingProjectCount ?? 0) > 0" square outline color="grey-8">Project: {{ dataQuality.missingProjectCount }}</q-chip>
          <q-chip v-if="(dataQuality?.missingOwnershipTypeCount ?? 0) > 0" square outline color="grey-8">Ownership: {{ dataQuality.missingOwnershipTypeCount }}</q-chip>
          <q-chip v-if="(dataQuality?.missingPaymentTermCount ?? 0) > 0" square outline color="grey-8">Payment Term: {{ dataQuality.missingPaymentTermCount }}</q-chip>
          <q-chip v-if="(dataQuality?.missingPriceCount ?? 0) > 0" square outline color="grey-8">Price: {{ dataQuality.missingPriceCount }}</q-chip>
          <q-chip v-if="(dataQuality?.missingEndDateCount ?? 0) > 0" square outline color="grey-8">End Date: {{ dataQuality.missingEndDateCount }}</q-chip>
        </div>
      </q-card-section>
      <q-separator />
      <q-table
        flat dense
        :rows="dataQuality?.items || []"
        :columns="dataQualityColumns"
        row-key="serviceId"
        :pagination="{ rowsPerPage: 8 }"
        no-data-label="All in-scope services have complete billing data"
      >
        <template #body-cell-missingFields="props">
          <q-td :props="props">
            <q-chip v-for="f in props.row.missingFields" :key="f" dense square color="orange-1" text-color="orange-9" class="q-mr-xs">{{ f }}</q-chip>
          </q-td>
        </template>
      </q-table>
    </q-card>

    <!-- ===================== Renewals ===================== -->
    <div class="row items-center q-mb-sm q-mt-lg">
      <q-icon name="o_event_repeat" color="primary" size="22px" class="q-mr-sm" />
      <span class="text-h6 text-primary text-weight-bold">Upcoming Renewals &amp; End Dates</span>
      <q-space />
      <q-btn-toggle
        v-model="filter.upcomingDays"
        no-caps
        rounded
        unelevated
        toggle-color="primary"
        color="grey-3"
        text-color="grey-9"
        size="sm"
        :options="[{ label: 'Next 30d', value: 30 }, { label: 'Next 60d', value: 60 }, { label: 'Next 90d', value: 90 }]"
        @update:model-value="onRenewalWindowChange"
      />
    </div>
    <div class="row q-col-gutter-md">
      <div class="col-12 col-md-7">
        <q-card flat bordered>
          <q-card-section class="q-py-sm">
            <div class="text-subtitle2 text-primary">Renewing within {{ renewals?.upcomingDays ?? filter.upcomingDays }} days
              <q-badge color="primary" class="q-ml-xs">{{ renewals?.upcoming?.length ?? 0 }}</q-badge>
            </div>
          </q-card-section>
          <q-separator />
          <q-table
            flat dense
            :rows="renewals?.upcoming || []"
            :columns="renewalColumns"
            row-key="serviceId"
            :pagination="{ rowsPerPage: 8 }"
            no-data-label="No services renewing in the selected window"
          >
            <template #body-cell-endDate="props">
              <q-td :props="props">{{ toDate(props.row.endDate) }}</q-td>
            </template>
            <template #body-cell-daysUntilRenewal="props">
              <q-td :props="props" class="text-right">
                <q-badge :color="renewalBadgeColor(props.row.daysUntilRenewal)">{{ props.row.daysUntilRenewal }}d</q-badge>
              </q-td>
            </template>
            <template #body-cell-annualized="props">
              <q-td :props="props" class="text-right">{{ money2(props.row.annualized) }}</q-td>
            </template>
          </q-table>
        </q-card>
      </div>
      <div class="col-12 col-md-5">
        <q-card flat bordered>
          <q-card-section class="q-py-sm">
            <div class="text-subtitle2 text-orange-9">
              <q-icon name="o_warning_amber" class="q-mr-xs" />Recurring — No End Date
              <q-badge color="orange" class="q-ml-xs">{{ renewals?.recurringWithoutEndDate?.length ?? 0 }}</q-badge>
            </div>
          </q-card-section>
          <q-separator />
          <q-table
            flat dense
            :rows="renewals?.recurringWithoutEndDate || []"
            :columns="recurringNoEndColumns"
            row-key="serviceId"
            :pagination="{ rowsPerPage: 8 }"
            no-data-label="All recurring services have an end date"
          >
            <template #body-cell-annualized="props">
              <q-td :props="props" class="text-right">{{ money2(props.row.annualized) }}</q-td>
            </template>
          </q-table>
        </q-card>
      </div>
    </div>

    <div class="q-mt-lg" />
  </q-page>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from "vue";
import useFilters from "composables/useFilters";
import { notifyError } from "assets/utils";
import infraDashboardService from "modules/infra-dashboard/infraDashboard.service";
import commonService from "services/common.service";
import projectService from "modules/project/projects.service";
import { useMultiSelectDropdown } from "composables/form-inputs/useDropdown.js";
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";
import kpiCard from "modules/infra-dashboard/components/kpiCard.vue";
import chartPanel from "modules/infra-dashboard/components/chartPanel.vue";

const { toCurrency, toDate, toPercentage } = useFilters();

// -----------------------------------------------------------------------------
// State
// -----------------------------------------------------------------------------
const loading = ref(false);
const showFilter = ref(false);

const summary = ref(null);
const breakdowns = ref(null);
const priceChanges = ref([]);
const history = ref(null);
const dataQuality = ref(null);
const renewals = ref(null);

const filter = reactive({
  providerIds: [],
  projectIds: [],
  itemTypeIds: [],
  ownershipTypeIds: [],
  paymentTermIds: [],
  fromDate: "",
  toDate: "",
  upcomingDays: 30
});

// -----------------------------------------------------------------------------
// Filter option loaders (DropDown master data + projects)
// -----------------------------------------------------------------------------
const providerOptions = useMultiSelectDropdown(commonService.getDropDown, { labelKey: "dropdownValue", valueKey: "id" });
const itemTypeOptions = useMultiSelectDropdown(commonService.getDropDown, { labelKey: "dropdownValue", valueKey: "id" });
const ownershipOptions = useMultiSelectDropdown(commonService.getDropDown, { labelKey: "dropdownValue", valueKey: "id" });
const paymentTermOptions = useMultiSelectDropdown(commonService.getDropDown, { labelKey: "dropdownValue", valueKey: "id" });
const projectOptions = useMultiSelectDropdown(projectService.getProjectsListForDropdown, { labelKey: "text", valueKey: "value" });

// -----------------------------------------------------------------------------
// Formatting helpers
// -----------------------------------------------------------------------------
function money0 (v) {
  return (v === null || v === undefined) ? "—" : toCurrency(v, 0);
}
function money2 (v) {
  return (v === null || v === undefined) ? "—" : toCurrency(v, 2);
}
function pct (v) {
  return (v === null || v === undefined) ? "" : toPercentage(v, 1);
}

// -----------------------------------------------------------------------------
// Derived
// -----------------------------------------------------------------------------
const activeFilterCount = computed(() =>
  (filter.providerIds?.length || 0) +
  (filter.projectIds?.length || 0) +
  (filter.itemTypeIds?.length || 0) +
  (filter.ownershipTypeIds?.length || 0) +
  (filter.paymentTermIds?.length || 0) +
  (filter.fromDate ? 1 : 0) +
  (filter.toDate ? 1 : 0)
);

const trendSubtitle = computed(() => {
  if (!history.value) return "";
  return `Recurring in range: ${money0(history.value.rangeRecurringTotal)} • One-time: ${money0(history.value.rangeOneTimeTotal)}`;
});

const ytdVsYearlySeries = computed(() => [
  summary.value?.ytdTotal || 0,
  summary.value?.annualizedTotal || 0
]);

const yearlyMonthlySub = computed(() =>
  `≈ ${money0((summary.value?.annualizedTotal || 0) / 12)} / month`
);

const ytdYearlySubtitle = computed(() => {
  const y = summary.value?.annualizedTotal || 0;
  return `Yearly ${money0(y)} · Monthly ${money0(y / 12)} · YTD ${money0(summary.value?.ytdTotal || 0)}`;
});

function renewalBadgeColor (days) {
  if (days === null || days === undefined) return "grey";
  if (days <= 15) return "negative";
  if (days <= 45) return "orange";
  return "primary";
}

// -----------------------------------------------------------------------------
// Table columns
// -----------------------------------------------------------------------------
const priceChangeColumns = [
  { name: "serviceName", label: "Service", field: "serviceName", align: "left", sortable: true },
  { name: "accountName", label: "Account", field: "accountName", align: "left", sortable: true },
  { name: "providerLabel", label: "Provider", field: "providerLabel", align: "left", sortable: true },
  { name: "paymentTermLabel", label: "Payment Term", field: "paymentTermLabel", align: "left" },
  { name: "previousPrice", label: "Previous", field: "previousPrice", align: "right", sortable: true },
  { name: "currentPrice", label: "Current", field: "currentPrice", align: "right", sortable: true },
  { name: "absoluteChange", label: "Change", field: "absoluteChange", align: "right", sortable: true },
  { name: "percentageChange", label: "% Change", field: "percentageChange", align: "right", sortable: true },
  { name: "changedOn", label: "Changed On", field: "changedOn", align: "left", sortable: true }
];

const dataQualityColumns = [
  { name: "serviceName", label: "Service", field: "serviceName", align: "left", sortable: true },
  { name: "accountName", label: "Account", field: "accountName", align: "left", sortable: true },
  { name: "missingFields", label: "Missing Billing Data", field: "missingFields", align: "left" }
];

const renewalColumns = [
  { name: "serviceName", label: "Service", field: "serviceName", align: "left", sortable: true },
  { name: "accountName", label: "Account", field: "accountName", align: "left", sortable: true },
  { name: "customerLabel", label: "Client", field: "customerLabel", align: "left" },
  { name: "providerLabel", label: "Provider", field: "providerLabel", align: "left" },
  { name: "paymentTermLabel", label: "Payment Term", field: "paymentTermLabel", align: "left" },
  { name: "endDate", label: "End Date", field: "endDate", align: "left", sortable: true },
  { name: "daysUntilRenewal", label: "Days", field: "daysUntilRenewal", align: "right", sortable: true },
  { name: "annualized", label: "Annualized", field: "annualized", align: "right", sortable: true }
];

const recurringNoEndColumns = [
  { name: "serviceName", label: "Service", field: "serviceName", align: "left", sortable: true },
  { name: "accountName", label: "Account", field: "accountName", align: "left", sortable: true },
  { name: "providerLabel", label: "Provider", field: "providerLabel", align: "left" },
  { name: "paymentTermLabel", label: "Payment Term", field: "paymentTermLabel", align: "left" },
  { name: "annualized", label: "Annualized", field: "annualized", align: "right", sortable: true }
];

// -----------------------------------------------------------------------------
// Data loading
// -----------------------------------------------------------------------------
function buildParams () {
  const params = {
    providerIds: filter.providerIds,
    projectIds: filter.projectIds,
    itemTypeIds: filter.itemTypeIds,
    ownerShipTypeIds: filter.ownershipTypeIds,
    paymentTermIds: filter.paymentTermIds,
    upcomingDays: filter.upcomingDays
  };
  if (filter.fromDate) params.fromDate = filter.fromDate;
  if (filter.toDate) params.toDate = filter.toDate;
  return params;
}

async function refreshAll () {
  loading.value = true;
  const params = buildParams();
  try {
    const results = await Promise.allSettled([
      infraDashboardService.getSummary(params),
      infraDashboardService.getBreakdowns(params),
      infraDashboardService.getPriceChanges(params),
      infraDashboardService.getHistory(params),
      infraDashboardService.getDataQuality(params),
      infraDashboardService.getRenewals(params)
    ]);

    summary.value = results[0].status === "fulfilled" ? results[0].value : summary.value;
    breakdowns.value = results[1].status === "fulfilled" ? results[1].value : breakdowns.value;
    priceChanges.value = results[2].status === "fulfilled" ? (results[2].value || []) : priceChanges.value;
    history.value = results[3].status === "fulfilled" ? results[3].value : history.value;
    dataQuality.value = results[4].status === "fulfilled" ? results[4].value : dataQuality.value;
    renewals.value = results[5].status === "fulfilled" ? results[5].value : renewals.value;

    if (results.some((r) => r.status === "rejected")) {
      notifyError({ message: "Some dashboard sections could not be loaded." });
    }
  } catch {
    notifyError({ message: "Failed to load the infrastructure dashboard." });
  } finally {
    loading.value = false;
  }
}

async function onRenewalWindowChange () {
  try {
    renewals.value = await infraDashboardService.getRenewals(buildParams());
  } catch {
    notifyError({ message: "Failed to update renewals." });
  }
}

function onApply () {
  showFilter.value = false;
  refreshAll();
}

function onClearFilters () {
  filter.providerIds = [];
  filter.projectIds = [];
  filter.itemTypeIds = [];
  filter.ownershipTypeIds = [];
  filter.paymentTermIds = [];
  filter.fromDate = "";
  filter.toDate = "";
  filter.upcomingDays = 30;
  showFilter.value = false;
  refreshAll();
}

onMounted(() => {
  providerOptions.load("Account Provider Type");
  itemTypeOptions.load("Account Item Type");
  ownershipOptions.load("Ownership Type");
  paymentTermOptions.load("Payment Term");
  projectOptions.load(false, true, true);
  refreshAll();
});
</script>

<style scoped>
.drilldown {
  border: 1px solid #e0e0e0;
  border-radius: 8px;
}
</style>
