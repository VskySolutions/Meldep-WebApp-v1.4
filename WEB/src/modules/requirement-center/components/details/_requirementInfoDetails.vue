<template>
  <q-card flat bordered class="dashboard-card" style="border: 0.5px solid #1b75ab;">
    <div class="row q-col-gutter-lg q-pa-sm">
      <div class="col-12 col-md-6">
        <q-list dense>
          
          <q-item>
            <q-item-section>
              <div class="text-caption text-grey">
                Requirement No
              </div>
              <div class="q-mb-sm">
                {{ model.requirementNumber || "-" }}
              </div>
            </q-item-section>
          </q-item>

          <q-item>
            <q-item-section>
              <div class="text-caption text-grey">
                Requirement
              </div>
              <div class="q-mb-sm">
                {{ model.title || "-" }}
              </div>
            </q-item-section>
          </q-item>
          
          <q-item>
            <q-item-section>
              <div class="text-caption text-grey">
                Project Name
              </div>
              <div class="q-mb-sm">
                {{ model.project.name || "-" }}
              </div>
            </q-item-section>
          </q-item>

          <q-item>
            <q-item-section>
              <div class="text-caption text-grey">
                Project Module Name
              </div>
              <div class="q-mb-sm">
                {{ model.projectModule.name || "-" }}
              </div>
            </q-item-section>
          </q-item>

          <q-item>
            <q-item-section>
              <div class="text-caption text-grey">
                Type
              </div>
              <div class="q-mb-sm">
                {{ model.requirementType.dropDownValue || "-" }}
              </div>
            </q-item-section>
          </q-item>

          <q-item>
            <q-item-section>
              <div class="text-caption text-grey">
                Area
              </div>
              <div class="q-mb-sm">
                {{ model.area.dropDownValue || "-" }}
              </div>
            </q-item-section>
          </q-item>

          <q-item>
            <q-item-section>
              <div class="text-caption text-grey">
                Workspace
              </div>
              <div class="q-mb-sm">
                {{ model.workspace.dropDownValue || "-" }}
              </div>
            </q-item-section>
          </q-item>

          <q-item>
            <q-item-section>
              <div class="text-caption text-grey">
                Task
              </div>
              <div class="q-mb-sm">
                <span v-if="model.projectTaskRelatedMappings?.length">
                  <template v-for="(item, index) in model.projectTaskRelatedMappings" :key="index">
                    <span class="hoverable-cell" style="cursor: pointer;" @click="onViewTask(item.id)">
                      #{{ item.projectTaskNumber }}
                      <span v-if="item.status">
                        ({{ item.status.dropDownValue }})
                      </span>
                    </span>
                    <span v-if="index < model.projectTaskRelatedMappings.length - 1">, </span>
                  </template>
                </span>
              </div>
            </q-item-section>
          </q-item>

          <q-item>
            <q-item-section>
              <div class="text-caption text-grey">
                Draft/Confirmed
              </div>
              <div class="q-mb-sm">
                {{ model.editingStatus === 1  ? 'Draft' : 'Confirmed'  }}
              </div>
            </q-item-section>
          </q-item>

          <q-item>
            <q-item-section>
              <div class="text-caption text-grey">
                Requirement Identified User Type
              </div>
              <div class="q-mb-sm">
                {{ model.userType.dropDownValue || "-" }}
              </div>
            </q-item-section>
          </q-item>
          
          <q-item>
            <q-item-section>
              <div class="text-caption text-grey">
                Requirement Identified By
              </div>
              <div class="q-mb-sm">
                {{ model.userType.dropDownValue === 'Customer' ? (model.customer && model.customer.fullName ? model.customer.fullName : 'N/A') : (model.employee && model.employee.person && model.employee.person.fullName ? model.employee.person.fullName : 'N/A') || "-" }}
              </div>
            </q-item-section>
          </q-item>

          <q-item>
            <q-item-section>
              <div class="text-caption text-grey">
                Requirement Entered By
              </div>
              <div class="q-mb-sm">
                {{ model.requirementEntered.person.fullName || "-" }}
              </div>
            </q-item-section>
          </q-item>

          <q-item>
            <q-item-section>
              <div class="text-caption text-grey">
                Requirement Status
              </div>
              <div class="q-mb-sm">
                {{ model.status.dropDownValue || "-" }}
              </div>
            </q-item-section>
          </q-item>

          <q-item>
            <q-item-section>
              <div class="text-caption text-grey">
                Last Note
              </div>
              <div class="q-mb-sm">
                <span v-html="model.lastNote" />
              </div>
            </q-item-section>
          </q-item>

        </q-list>
      </div>
      <div class="col-12 col-md-6">
        <q-item>
          <q-item-section>
            <div class="text-caption text-grey">
              Approval Status
            </div>
            <div class="q-mb-sm">
              {{ model.approvalStatusDropDown.dropDownValue || "-" }}
            </div>
          </q-item-section>
        </q-item>

        <q-item>
          <q-item-section>
            <div class="text-caption text-grey">
              Planned Start Date
            </div>
            <div class="q-mb-sm">
              {{ model.plannedStartDate || "-" }}
            </div>
          </q-item-section>
        </q-item>

        <q-item>
          <q-item-section>
            <div class="text-caption text-grey">
              Planned End Date
            </div>
            <div class="q-mb-sm">
              {{ model.plannedEndDate || "-" }}
            </div>
          </q-item-section>
        </q-item>

        <q-item>
          <q-item-section>
            <div class="text-caption text-grey">
              Actual Start Date
            </div>
            <div class="q-mb-sm">
              {{ model.actualStartDate || "-" }}
            </div>
          </q-item-section>
        </q-item>

        <q-item>
          <q-item-section>
            <div class="text-caption text-grey">
              Actual End Date
            </div>
            <div class="q-mb-sm">
              {{ model.actualEndDate || "-" }}
            </div>
          </q-item-section>
        </q-item>

        <!-- <q-item>
          <q-item-section>
            <div class="text-caption text-grey hidden">
              Confirmed By
            </div>
            <div class="q-mb-sm">
              {{ model.confirmedBy.person.fullName || "-" }}
            </div>
          </q-item-section>
        </q-item> -->
        
        <q-item>
          <q-item-section>
            <div class="text-caption text-grey">
              Approved By
            </div>
            <div class="q-mb-sm">
              {{ model.approvedBy.person.fullName || "-" }}
            </div>
          </q-item-section>
        </q-item>
        
        <q-item>
          <q-item-section>
            <div class="text-caption text-grey">
              Requirement Close Date
            </div>
            <div class="q-mb-sm">
              {{ model.closeDate || "-" }}
            </div>
          </q-item-section>
        </q-item>

        <q-item>
          <q-item-section>
            <div class="text-caption text-grey">
              Created By
            </div>
            <div class="q-mb-sm">
              {{ model.createdBy.person.firstName + " "+ model.createdBy.person.lastName || "-" }}
            </div>
          </q-item-section>
        </q-item>

        <q-item>
          <q-item-section>
            <div class="text-caption text-grey">
              Created Date
            </div>
            <div class="q-mb-sm">
              {{ model.createdOnUtc || "-" }}
            </div>
          </q-item-section>
        </q-item>
        
        <q-item>
          <q-item-section>
            <div class="text-caption text-grey">
              Updated By
            </div>
            <div class="q-mb-sm">
              {{ model.updatedBy?.person?.firstName && model.updatedBy?.person?.lastName
                ? model.updatedBy.person.firstName + " " + model.updatedBy.person.lastName
                : "-" }}
            </div>
          </q-item-section>
        </q-item>

        <q-item>
          <q-item-section>
            <div class="text-caption text-grey">
              Updated Date
            </div>
            <div class="q-mb-sm">
              {{ model.updatedOnUtc || "-" }}
            </div>
          </q-item-section>
        </q-item>

      </div>
    </div>

  </q-card>
</template>

<script setup>
import { ref, onMounted, watch } from "vue";
import _ from "lodash";
import requirementService from "modules/requirement/requirement.service";

const props = defineProps({
  requirementId: {
    type: String,
    required: true
  }
});

const loading = ref(false);

// Define model values
const model = ref({
  title: "",
  notes: "",
  employeeId: "",
  IdentifiedDate: "",
  approvalStatus: "",
  description: "",
  createdOnUtc: "",
  lastNote: "",
  project: {
    name: ""
  },
  projectModule: {
    name: ""
  },
  requirementType: {
    dropDownValue: ""
  },
  area: {
    dropDownValue: ""
  },
  workspace: {
    dropDownValue: ""
  },
  customer: {
    fullName: ""
  },
  status: {
    dropDownValue: ""
  },
  userType: {
    dropDownValue: ""
  },
  requirementEntered: {
    person: {
      fullName: ""
    }
  },
  confirmedBy: {
    person: {
      fullName: ""
    }
  },
  approvedBy: {
    person: {
      fullName: ""
    }
  },
  employee: {
    person: {
      fullName: ""
    }
  },
  approvalStatusDropDown: {
    dropDownValue: ""
  },
  createdBy: {
    person: {
      firstName: "",
      lastName: ""
    }
  },
  updatedBy: {
    person: {
      firstName: "",
      lastName: ""
    }
  }
});

// get get Requirement on edit mode
const getRequirement = () => {
  loading.value = true;
  requirementService.getRequirementDetails(props.requirementId).then((resp) => {
    model.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

watch(
  () => props.requirementId,
  async () => {
    await getRequirement();
  },
  {
    immediate: true
  }
);

onMounted(async () => {
  await getRequirement();
});
</script>
