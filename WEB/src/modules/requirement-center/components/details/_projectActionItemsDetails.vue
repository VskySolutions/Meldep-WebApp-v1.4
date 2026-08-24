<template>
  <q-card flat bordered>

    <!-- Header -->
    <q-card-section class="row bg-primary text-white q-pa-sm items-center justify-between">
      <div class="row items-center">
        <q-icon
          name="o_task_alt"
          size="sm"
          class="q-mr-md"
        />
        <div class="column">
          <div class="text-h6 text-weight-bold">
            {{ model.title }}
          </div>
        </div>
      </div>
    </q-card-section>

    <q-separator />
    <!-- DETAILS -->
      <div class="row q-col-gutter-lg">
        <div class="col-12 col-md-6">
          <q-list dense>

            <!-- <q-item>
              <q-item-section>
                <div class="text-caption text-grey">
                  Project Name
                </div>
                <div class="q-mb-sm">
                  {{ model.project?.name || "-" }}
                </div>
              </q-item-section>
            </q-item> -->

            <q-item>
              <q-item-section>
                <div class="text-caption text-grey">
                  Title
                </div>
                <div class="q-mb-sm">{{ model.title || "-" }}</div>
              </q-item-section>
            </q-item>

            <q-item>
              <q-item-section>
                <div class="text-caption text-grey">
                  Customer
                </div>
                <div class="q-mb-sm">{{ model.customer?.name || "-" }}</div>
              </q-item-section>
            </q-item>

            <q-item>
              <q-item-section>
                <div class="text-caption text-grey">
                  Priority
                </div>
                <div class="q-mb-sm">{{ model.priority?.dropDownValue || "-" }}</div>
              </q-item-section>
            </q-item>
            
            <q-item>
              <q-item-section>
                <div class="text-caption text-grey">
                  Created By
                </div>
                <div class="q-mb-sm">{{ model.createdBy?.person ? model.createdBy.person.fullName : "-" }}</div>
              </q-item-section>
            </q-item>
            
            <q-item>
              <q-item-section>
                <div class="text-caption text-grey">
                  Updated By
                </div>
                <div class="q-mb-sm">{{ model.updatedBy?.person ? model.updatedBy.person.fullName : "-" }}</div>
              </q-item-section>
            </q-item>
          </q-list>
        </div>
        <div class="col-12 col-md-6">
          <q-list dense>   
            <q-item>
              <q-item-section></q-item-section>
            </q-item>
            <q-item>
              <q-item-section>
                <div class="text-caption text-grey">
                  Employee
                </div>
                <div class="q-mb-sm">
                  {{
                    model.employee?.person
                      ? `${model.employee?.person?.fullName}`
                      : '-'
                  }}
                </div>
              </q-item-section>
            </q-item>

            <q-item>
              <q-item-section>
                <div class="text-caption text-grey">
                  Due Date
                </div>
                <div class="q-mb-sm">{{ model.dueDate || "-" }}</div>
              </q-item-section>
            </q-item>

            <q-item>
              <q-item-section>
                <div class="text-caption text-grey">
                  Created Date
                </div>
                <div class="q-mb-sm">{{ model.createdOnUtc ? model.createdOnUtc : "-" }}</div>
              </q-item-section>
            </q-item>

            <q-item>
              <q-item-section>
                <div class="text-caption text-grey">
                  Updated Date
                </div>
                <div class="q-mb-sm">{{ model.updatedOnUtc ? model.updatedOnUtc : "-" }}</div>
              </q-item-section>
            </q-item>
          </q-list>
        </div>
      </div>
      <div class="col-12 q-ml-md">
        <div class="text-caption text-grey q-mb-sm">Description</div>

        <div
          class="RichTextEditor"
          v-html="model.description || '-'"
        />
      </div>

  </q-card>
</template>

<script setup>
import { ref, onMounted, watch } from "vue";
import _ from "lodash";

import projectActionItemsService from "modules/project-action-items/projectActionItems.service";

const props = defineProps({
  id: {
    type: String,
    required: true
  }
});

const loading = ref(false);

const model = ref({
  project: {},
  title: {},
  description: {},
  dueDate: {},
  employee: { person: {} },
  customer: {},
  priority: {},
  createdBy: { person: {} },
  updatedBy: { person: {} },
  createdOnUtc: {},
  updatedOnUtc: {}
});

async function getProjectActionItemDetailsById() {
  if (!props.id) return;
  loading.value = true;

  try {
    const resp = await projectActionItemsService.getProjectActionItemDetailsById(props.id);

    model.value = _.cloneDeep(resp);
  } finally {
    loading.value = false;
  }
}

watch(
  () => props.id,
  async () => {
    await getProjectActionItemDetailsById();
  },
  {
    immediate: true
  }
);

onMounted(async () => {
  await getProjectActionItemDetailsById();
});
</script>
