<template>
  <q-dialog ref="dialogRef" class="customDialog" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 75vw !important;max-width: 75vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white" style="flex-grow: 1;">{{ model.name }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <fieldset v-if="!props.isAssign">
            <legend>Infra FTP Info</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Infra Account Service</div>
                <div class="text-black q-mb-cs">
                  {{ model.infraService?.name }}
                </div>
              </div>
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Protocol Type</div>
                <div class="text-black">
                  {{ model.protocolType.dropDownValue }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Encryption Type</div>
                <div class="text-black">
                  {{ model.encryptionType.dropDownValue }}
                </div>
              </div>
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Wallet Type</div>
                <div class="text-black">
                  {{ model.walletType.dropDownValue }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Wallet Number</div>
                <div class="text-black">
                  {{ model.walletNumber }}
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
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Host</div>
                <div class="text-black">
                  {{ model.host }}
                </div>
              </div>
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Port</div>
                <div class="text-black">
                  {{ model.port }}
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
            <legend>Assign Project Instance</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <formSingleSelectDropdown
                v-model="selectedProjectInstance"
                :options="infraProjectInstancePlatformDropdownSingleSelect.list.value"
                :filter="infraProjectInstancePlatformDropdownSingleSelect.filter"
                :placeholder="`Search and assign to project instance`"
                :option-disable="isProjectInstanceDisabled"
                @update:model-value="assignProjectInstance"
              />
            </div>
          </fieldset>
          <fieldset v-if="assignedProjectInstances?.length">
            <legend>Assigned Project Instance</legend>
            <div class="row q-col-gutter-x-md">
              <div class="col-12 q-mb-md">
                <div class="q-mb-sm">
                  <div class="row q-gutter-sm">
                    <q-chip
                      v-for="instance in assignedProjectInstances"
                      :key="instance.value"
                      :removable="props.isAssign"
                      color="primary"
                      text-color="white"
                      @remove="removeProjectInstance(instance)"
                    >
                      {{ instance.text }}
                    </q-chip>
                  </div>
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
import infraFTPService from "../infraFTP.service";
import infraProjectInstanceModule from "src/modules/infra-project-instance/utils/dropdowns.js";
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
  host: "",
  port: "",
  walletNumber: "",
  instructions: "",
  infraService: {
    name: ""
  },
  protocolType: {
    dropDownValue: ""
  },
  encryptionType: {
    dropDownValue: ""
  },
  walletType: {
    dropDownValue: ""
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

const getInfraFTPInDetailById = async () => {
  try {
    loading.value = true;
    const resp = await infraFTPService.getInfraFTPInDetailById(props.id);
    model.value = _.cloneDeep(resp);

    assignedProjectInstances.value =
      resp.infraFTPsProjectInstanceMapping?.map(p => ({
        text: p.infraProjectInstance.platform.dropDownValue + " - " + p.infraProjectInstance.url,
        value: p.infraProjectInstance.id,
        id: p.id
      })) || [];
  } catch (error) {
    console.error("Failed to load Infra FTP:", error);
  } finally {
    loading.value = false;
  }
};

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const { infraProjectInstancePlatformDropdownSingleSelect } = infraProjectInstanceModule();

// =====================================================================================
// Assign/Removed project instance
// =====================================================================================
const assignedProjectInstances = ref([]);
const selectedProjectInstance = ref(null);

function isProjectInstanceDisabled (opt) {
  return assignedProjectInstances.value.some(p => p.value === opt.value);
}

async function assignProjectInstance (projectInstanceId) {
  if (!projectInstanceId) return;

  const projectInstance = infraProjectInstancePlatformDropdownSingleSelect.list.value.find(p => p.value === projectInstanceId);

  const response = await infraFTPService.infraFTPAssignToProjectInstance(props.id, projectInstanceId);
  assignedProjectInstances.value.push({
    ...projectInstance,
    id: response
  });

  notifySuccess({ message: "Project instance assigned successfully." });

  selectedProjectInstance.value = null;
}

async function removeProjectInstance (projectInstance) {
  await infraFTPService.deleteAssignProjectInstance(projectInstance.id);

  assignedProjectInstances.value =
    assignedProjectInstances.value.filter(p => p.value !== projectInstance.value);

  notifySuccess({ message: "Project instance removed successfully." });
}

// On page rendering
onMounted(() => {
  getInfraFTPInDetailById();
  infraProjectInstancePlatformDropdownSingleSelect.load();
});

</script>
<style>
.break-url {
  word-break: break-word;
  overflow-wrap: anywhere;
  white-space: normal;
}
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
</style>
