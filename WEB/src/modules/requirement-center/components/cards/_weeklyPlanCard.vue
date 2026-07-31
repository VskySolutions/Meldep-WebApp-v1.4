<template>
  <q-card flat bordered class="dashboard-card">

    <!-- Header -->
    <q-card-section class="row items-center justify-between q-pb-sm">

      <div class="row items-center">

        <q-avatar
          rounded
          color="deep-purple-1"
          text-color="deep-purple"
          icon="o_date_range"
          size="36px"
        />

        <div class="q-ml-md row items-center">

          <div class="text-subtitle1 text-weight-bold">
            Weekly Plan
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
        {{ completion }}% completed
      </div>

    </q-card-section>

    <q-separator />

    <!-- Table -->

    <q-table
      flat
      :rows="rows"
      :columns="columns"
      row-key="week"
      hide-pagination
      :rows-per-page-options="[0]"
      class="weekly-table"
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

    <q-separator />

    <q-card-actions align="left">

      <q-btn
        flat
        no-caps
        color="primary"
        label="View all weeks"
        icon-right="o_expand_more"
      />

    </q-card-actions>

  </q-card>
</template>

<script setup>
import { computed } from 'vue'

const columns = [
  {
    name: 'week',
    label: 'WEEK',
    field: 'week',
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
    name: 'status',
    label: 'STATUS',
    field: 'status',
    align: 'left'
  }
]

const rows = [
  {
    week: 'Week 1',
    planned: 20,
    actual: 18,
    status: 'Completed'
  },
  {
    week: 'Week 2',
    planned: 22,
    actual: 15,
    status: 'In Progress'
  },
  {
    week: 'Week 3',
    planned: 18,
    actual: 0,
    status: 'Upcoming'
  },
  {
    week: 'Week 4',
    planned: 20,
    actual: 0,
    status: 'Upcoming'
  }
]

const completion = computed(() => {
  const planned = rows.reduce((sum, row) => sum + row.planned, 0)
  const actual = rows.reduce((sum, row) => sum + row.actual, 0)

  return Math.round((actual / planned) * 100)
})

function statusColor(status) {
  switch (status) {
    case 'Completed':
      return 'green-1'

    case 'In Progress':
      return 'blue-1'

    case 'Upcoming':
      return 'grey-3'

    case 'Delayed':
      return 'red-1'

    default:
      return 'grey-2'
  }
}

function statusTextColor(status) {
  switch (status) {
    case 'Completed':
      return 'positive'

    case 'In Progress':
      return 'primary'

    case 'Upcoming':
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

.weekly-table :deep(thead th) {
  font-size: 12px;
  font-weight: 600;
  color: #64748b;
}

.weekly-table :deep(tbody td) {
  height: 52px;
}
</style>
