<template>
  <q-dialog ref="dialogRef" class="customDialog" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important;max-width: 50vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white" style="flex-grow: 1;">{{ model.name }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <fieldset v-if="!props.isAssign">
            <legend>Infra Account Service Info</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Account</div>
                <div class="text-black q-mb-cs">
                  {{ model.infraAccount.name }}
                </div>
              </div>
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Item Type</div>
                <div class="text-black q-mb-cs">
                  {{ model.itemType.dropDownValue }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col">
                <div class="col-12 col-md-6">Ownership Type</div>
                <div class="text-black">
                  {{ model.ownerShipType.dropDownValue }}
                </div>
              </div>
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Name</div>
                <div class="text-black q-mb-cs">
                  {{ model.name }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-12">
                <div class="col-12 col-md-6" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;">URL</div>
                <div class="text-black break-url">
                  <a
                    :href="model.url"
                    target="_blank"
                    class="link-text"
                  >
                    {{ model.url }}
                  </a>
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Payment Term</div>
                <div class="text-black">
                  {{ model.paymentTerm.dropDownValue }}
                </div>
              </div>
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Price In Dollar</div>
                <div class="text-black RichTextEditor">
                  ${{ model.priceInDollar }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Start Date</div>
                <div class="text-black">
                  {{ model.startDate }}
                </div>
              </div>
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">End Date</div>
                <div class="text-black">
                  {{ model.endDate }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Wallet Type</div>
                <div class="text-black RichTextEditor">
                  {{ model.walletType.dropDownValue }}
                </div>
              </div>
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Wallet Number</div>
                <div class="text-black RichTextEditor">
                  {{ model.walletNumber }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Infra Account Service</div>
                <div class="text-black RichTextEditor">
                  {{ model.infraAccountService.name }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-12" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">
                <div class="q-mb-xs">Instruction</div>
                <div class="text-black RichTextEditor">
                  <p v-html="model.instructions ? model.instructions : '-'" />
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Created By</div>
                <div class="text-black">
                  {{ model.createdBy.person.fullName ? model.createdBy.person.fullName : "-" }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Created Date</div>
                <div class="text-black">
                  {{ model.createdOnUtc ? model.createdOnUtc : "-" }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Updated By</div>
                <div class="text-black">
                  {{ model.updatedBy.person.fullName ? model.updatedBy.person.fullName : "-" }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Updated Date</div>
                <div class="text-black">
                  {{ model.updatedOnUtc ? model.updatedOnUtc : "-" }}
                </div>
              </div>
            </div>
          </fieldset>
          <fieldset v-else>
            <legend>Assign Project</legend>
            <div class="row q-col-gutter-x-md">
              <formSingleSelectDropdown
                v-model="selectedProject"
                :options="projectNameDropdownSingleSelect.list.value"
                :filter="projectNameDropdownSingleSelect.filter"
                :option-disable="isProjectDisabled"
                @update:model-value="assignProject"
              />
            </div>
          </fieldset>
          <fieldset v-if="assignedProjects?.length">
            <legend>Assigned Projects({{ assignedProjects.length }})</legend>
            <div class="row q-col-gutter-x-md">
              <div class="col-12 q-mb-md">
                <div class="q-mb-sm">
                  <div class="row q-gutter-sm">
                    <q-chip
                      v-for="project in assignedProjects"
                      :key="project.value"
                      :removable="props.isAssign"
                      color="primary"
                      text-color="white"
                      @remove="removeProject(project)"
                    >
                      {{ project.text }}
                    </q-chip>
                  </div>
                </div>
                <div class="q-mt-md text-subtitle2 text-weight-medium">
                  Actual cost per project:
                  <span class="text-dark">${{ model.actualPriceInDollar?.toFixed(2) || "0.00" }}</span>
                </div>
              </div>
            </div>
          </fieldset>
        </div>
      </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { ref, onMounted } from "vue";
import _ from "lodash";
import { notifySuccess } from "assets/utils";
import infraAccountServicesService from "../infraAccountServices.service";
import projectModule from "src/modules/project/utils/dropdowns.js";
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";

// Common variables
const loading = ref(true);

// Props values i.e. come from query string
const props = defineProps({
  id: { type: String, default: "" },
  isAssign: { type: Boolean, default: false }
});

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Define model values
const model = ref({
  name: "-",
  url: "",
  startDate: "",
  priceInDollar: "",
  walletNumber: "",
  instructions: "",
  infraAccount: {
    name: ""
  },
  project: {
    name: ""
  },
  itemType: {
    dropDownValue: ""
  },
  ownerShipType: {
    dropDownValue: ""
  },
  paymentTerm: {
    dropDownValue: ""
  },
  walletType: {
    dropDownValue: ""
  },
  infraAccountService: {
    name: ""
  },
  createdBy: {
    person: {
      fullName: ""
    }
  },
  updatedBy: {
    person: {
      fullName: ""
    }
  }
});

const getInfraAccountServicesInDetailById = async () => {
  try {
    loading.value = true;
    const resp = await infraAccountServicesService.getInfraAccountServicesInDetailById(props.id);
    model.value = _.cloneDeep(resp);

    assignedProjects.value =
      resp.infraProjectServices?.map(p => ({
        text: p.project.name,
        value: p.project.id,
        id: p.id
      })) || [];
  } catch (error) {
    console.error("Failed to load Infra Account:", error);
  } finally {
    loading.value = false;
  }
};

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const { projectNameDropdownSingleSelect } = projectModule();

// =====================================================================================
// Assign/Removed project
// =====================================================================================
const assignedProjects = ref([]);
const selectedProject = ref(null);

function isProjectDisabled (opt) {
  return assignedProjects.value.some(p => p.value === opt.value);
}

async function assignProject (projectId) {
  if (!projectId) return;

  const project = projectNameDropdownSingleSelect.list.value.find(p => p.value === projectId);

  const response = await infraAccountServicesService.infraServiceAssignToProject(props.id, projectId);
  assignedProjects.value.push({
    ...project,
    id: response
  });

  notifySuccess({ message: "Project assigned successfully." });

  selectedProject.value = null;
}

async function removeProject (project) {
  await infraAccountServicesService.deleteAssignProject(project.id);

  assignedProjects.value =
    assignedProjects.value.filter(p => p.value !== project.value);

  notifySuccess({ message: "Project removed successfully." });
}

// On page rendering
onMounted(() => {
  getInfraAccountServicesInDetailById();
  projectNameDropdownSingleSelect.load(false, true, true);
});

</script>
<style>
.break-url {
  word-break: break-word;
  overflow-wrap: anywhere;
  white-space: normal;
}
</style>
