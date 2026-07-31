<template>
  <q-card flat bordered class="dashboard-card">

    <!-- Header -->
    <q-card-section class="row items-center justify-between q-pb-sm">

      <div class="row items-center">

        <q-avatar
          rounded
          color="orange-1"
          text-color="warning"
          icon="o_calendar_month"
          size="36px"
        />

        <div class="q-ml-md row items-center">

          <div class="text-subtitle1 text-weight-bold">
            Monthly Plan
          </div>

          <q-badge
            rounded
            color="grey-3"
            text-color="grey-8"
            class="q-ml-sm"
          >
            {{ rows.length }}
          </q-badge>

        </div>

      </div>

      <div class="text-caption text-grey-7">
        {{ utilization }}% utilised (Jul)
      </div>

    </q-card-section>

    <q-separator />

    <!-- Table -->

    <q-table
      flat
      :rows="rows"
      :columns="columns"
      row-key="month"
      hide-pagination
      :rows-per-page-options="[0]"
      class="monthly-table"
    >

      <template #body-cell-planned="props">
        <q-td :props="props">
          {{ props.value }} hrs
        </q-td>
      </template>

      <template #body-cell-actual="props">
        <q-td :props="props">
          {{ props.value }} hrs
        </q-td>
      </template>

      <template #body-cell-variance="props">
        <q-td :props="props">
          {{ props.value }}
        </q-td>
      </template>

      <template #body-cell-status="props">
        <q-td :props="props">

          <q-chip
            dense
            size="sm"
            :color="statusColor(props.value)"
            :text-color="statusTextColor(props.value)"
          >
            {{ props.value }}
          </q-chip>

        </q-td>
      </template>

    </q-table>

  </q-card>
</template>

<script setup>
import { computed } from 'vue'

const columns = [
  {
    name: 'month',
    label: 'MONTH',
    field: 'month',
    align: 'left'
  },
  {
    name: 'planned',
    label: 'PLANNED',
    field: 'planned',
    align: 'left'
  },
  {
    name: 'actual',
    label: 'ACTUAL',
    field: 'actual',
    align: 'left'
  },
  {
    name: 'variance',
    label: 'VARIANCE',
    field: 'variance',
    align: 'left'
  },
  {
    name: 'status',
    label: 'STATUS',
    field: 'status',
    align: 'left'
  }
]

const rows = [
  {
    month: 'July 2026',
    planned: 80,
    actual: 62,
    variance: '-18 hrs',
    status: 'On Track'
  },
  {
    month: 'August 2026',
    planned: 60,
    actual: 0,
    variance: '—',
    status: 'Planned'
  }
]

const utilization = computed(() => {
  const july = rows[0]
  return Math.round((july.actual / july.planned) * 100)
})

function statusColor(status) {
  switch (status) {
    case 'On Track':
      return 'blue-1'

    case 'Planned':
      return 'grey-3'

    case 'Delayed':
      return 'red-1'

    default:
      return 'grey-2'
  }
}

function statusTextColor(status) {
  switch (status) {
    case 'On Track':
      return 'primary'

    case 'Planned':
      return 'grey-8'

    case 'Delayed':
      return 'negative'

    default:
      return 'grey-8'
  }
}
</script>

<style scoped>
.dashboard-card {
  border-radius: 12px;
}

.monthly-table :deep(thead th) {
  font-size: 12px;
  font-weight: 600;
  color: #64748b;
}

.monthly-table :deep(tbody td) {
  height: 52px;
}
</style>
