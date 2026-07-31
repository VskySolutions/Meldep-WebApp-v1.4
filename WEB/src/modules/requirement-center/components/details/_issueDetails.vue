<template>
  <q-card flat bordered>
    <!-- Header -->
    <q-card-section class="row bg-primary text-white q-pa-sm items-center justify-between">
      <div class="row items-center">
        <q-icon
          name="o_bug_report"
          size="sm"
          class="q-mr-md"
        />
        <div class="column">
          <div class="text-caption text-blue-2">
            #{{ model.issueNumber }} • ISSUE
          </div>

          <div class="text-h6 text-weight-bold">
            {{ model.name }}
          </div>
        </div>
      </div>
    </q-card-section>
    <q-separator />
    <q-card-section>
      <div class="row q-col-gutter-md">
        <div class="col-12 col-md-6">
          <div class="text-caption text-grey">Project</div>
          <div>{{ model.project?.name || "-" }}</div>
        </div>

        <div class="col-12 col-md-6">
          <div class="text-caption text-grey">Project Module</div>
          <div>{{ model.projectModule?.name || "-" }}</div>
        </div>

        <div class="col-12 col-md-6">
          <div class="text-caption text-grey">Requirement</div>
          <div>{{ model.requirement?.title || "-" }}</div>
        </div>

        <div class="col-12 col-md-6">
          <div class="text-caption text-grey">Area</div>
          <div>{{ model.area?.dropDownValue || "-" }}</div>
        </div>

        <div class="col-12 col-md-6">
          <div class="text-caption text-grey">Workspace</div>
          <div>{{ model.workspace?.dropDownValue || "-" }}</div>
        </div>

        <div class="col-12 col-md-6">
          <div class="text-caption text-grey">Assign To</div>
          <div>{{ model.employee?.person?.fullName !== ' ' ? model.employee?.person?.fullName : "-" }}</div>
        </div>

        <div class="col-12 col-md-6">
          <div class="text-caption text-grey">Reported By</div>
          <div>{{ model.reportedBy?.person?.fullName || "-" }}</div>
        </div>

        <div class="col-12 col-md-6">
          <div class="text-caption text-grey">Priority</div>
          <div>{{ model.priority?.dropDownValue || "-" }}</div>
        </div>

        <div class="col-12">
          <div class="text-caption text-grey">Task</div>

          <div v-if="model.projectTaskRelatedMappings?.length">
            <template
              v-for="(item, index) in model.projectTaskRelatedMappings"
              :key="item.taskId"
            >
              <span
                class="text-primary hoverable-cell cursor-pointer"
                @click="onProjectTaskView(item.taskId)"
              >
                #{{ item.projectTask?.projectTaskNumber }}
                <span v-if="item.projectTask?.status">
                  ({{ item.projectTask.status.dropDownValue }})
                </span>
              </span>

              <span v-if="index < model.projectTaskRelatedMappings.length - 1">
                ,
              </span>
            </template>
          </div>
          <div v-else>-</div>
        </div>

        <div class="col-12 col-md-6">
          <div class="text-caption text-grey">Created Date</div>
          <div>{{ model.createdOnUtc || "-" }}</div>
        </div>

        <div class="col-12">
          <div class="text-caption text-grey q-mb-sm">Description</div>

          <div
            class="RichTextEditor"
            v-html="model.description || '-'"
          />
        </div>
      </div>
    </q-card-section>
  </q-card>
</template>

<script setup>
import { onMounted, ref, watch } from 'vue'
import _ from 'lodash'

import issueService from 'modules/issue/issue.service'

// Shared Project Task Dialogs
import {
  initProjectTaskDialogs,
  onProjectTaskView
} from "src/modules/project-tasks/utils/dialogs.js";

const props = defineProps({
  id: {
    type: String,
    required: true
  }
})

const loading = ref(false);
const tab = ref('details');
const activeRowId = ref(null);

const model = ref({
  name: "",
  issueNumber: "",
  description: "",
  createdOnUtc: "",
  status: "",
  project: "",
  projectModule: "",
  requirement: "",
  area: "",
  workspace: "",
  priority: "",
  employee: {
    person: ""
  },
  reportedBy: {
    person: ""
  },
  projectTaskRelatedMappings: []
})

const getIssueDetails = async () => {
  if (!props.id) return

  loading.value = true;
  activeRowId.value = props.id;

  try {
    const resp = await issueService.getIssueDetails(props.id)
    model.value = _.cloneDeep(resp)
  } finally {
    loading.value = false
  }
}

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initProjectTaskDialogs(activeRowId);

watch(
  () => props.id,
  getIssueDetails,
  { immediate: true }
)

onMounted(getIssueDetails)
</script>
