<template>
  <q-dialog ref="dialogRef" class="customDialog" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important;max-width: 50vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white" style="flex-grow: 1;">{{ model.infraProject.name }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <fieldset>
            <legend>Infra Project Instance Info</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Infra Project</div>
                <div class="text-black q-mb-cs">
                  {{ model.infraProject?.name }}
                </div>
              </div>
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Instance Type</div>
                <div class="text-black">
                  {{ model.instanceType.dropDownValue }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Platform</div>
                <div class="text-black">
                  {{ model.platform.dropDownValue }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-12">
                <div class="q-mb-xs">URL</div>
                <div class="text-black">
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
          <fieldset v-if="roles && roles.length">
            <div class="row justify-end q-mb-sm">
              <q-input
                v-model="search"
                outlined
                dense
                debounce="300"
                placeholder="Search role or username"
                style="width: 250px"
              >
                <template #append>
                  <q-icon name="o_search" />
                </template>
              </q-input>
            </div>
            <q-markup-table flat bordered dense separator="cell">
              <thead class="bg-primary text-white">
                <tr>
                  <th class="text-left">Role Name</th>
                  <th class="text-left">Username</th>
                  <th class="text-left">Password</th>
                </tr>
              </thead>
              <tbody>
                <template v-for="role in filteredRoles" :key="role.id">
                  <tr
                    v-for="(user, userIndex) in role.infraProjectInstanceRoleUsers"
                    :key="user.id"
                  >
                    <!-- Role Name -->
                    <td style="width: 35%;">
                      <span v-if="userIndex === 0">
                        {{ role.roleName }}
                      </span>
                    </td>

                    <td style="width: 35%;">{{ user.userName }}</td>

                    <td style="width: 30%;">
                      <div class="row items-center justify-between no-wrap">

                        <span class="q-mr-sm">
                          {{ user.showPassword ? user.password : '••••••••' }}
                        </span>

                        <q-icon
                          :name="user.showPassword ? 'o_visibility_off' : 'o_visibility'"
                          class="cursor-pointer"
                          size="18px"
                          @click="user.showPassword = !user.showPassword"
                        >
                          <q-tooltip>
                            {{ user.showPassword ? 'Hide Password' : 'Show Password' }}
                          </q-tooltip>
                        </q-icon>

                      </div>
                    </td>

                  </tr>
                </template>
              </tbody>
            </q-markup-table>
          </fieldset>
        </div>
      </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { ref, onMounted, computed } from "vue";
import _ from "lodash";
import infraProjectInstanceService from "../infraProjectInstance.service";

// Common variables
const loading = ref(true);
const roles = ref(null);
const search = ref("");

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Define model values
const model = ref({
  infraProject: {
    name: ""
  },
  instanceType: {
    dropDownValue: ""
  },
  platform: {
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

const getInfraProjectInstanceInDetailById = async () => {
  try {
    loading.value = true;
    const resp = await infraProjectInstanceService.getInfraProjectInstanceInDetailById(props.id);
    model.value = _.cloneDeep(resp);
    roles.value = resp.infraProjectInstanceRole.map(role => ({
      ...role,
      infraProjectInstanceRoleUsers: role.infraProjectInstanceRoleUsers.map(user => ({
        ...user,
        showPassword: false
      }))
    }));
  } catch (error) {
    console.error("Failed to load Infra Project Instance:", error);
  } finally {
    loading.value = false;
  }
};

const filteredRoles = computed(() => {
  if (!search.value) return roles.value;

  const keyword = search.value.toLowerCase();

  return roles.value
    .map(role => {
      const users = role.infraProjectInstanceRoleUsers.filter(user =>
        role.roleName?.toLowerCase().includes(keyword) ||
        user.userName?.toLowerCase().includes(keyword)
      );

      return users.length
        ? { ...role, infraProjectInstanceRoleUsers: users }
        : null;
    })
    .filter(Boolean);
});

// On page rendering
onMounted(() => {
  getInfraProjectInstanceInDetailById();
});

</script>
<style>
.break-url {
  word-break: break-word;
  overflow-wrap: anywhere;
  white-space: normal;
}
</style>
