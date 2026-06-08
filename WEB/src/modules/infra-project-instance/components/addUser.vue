<template>
  <q-dialog ref="dialogRef" class="customDialog q-pa-none dialog-scrollable-content" full-height position="right">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 70vw; max-width: 70vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">Add Users</div>
        <q-btn icon="o_close" class="close" color="white" flat round dense @click="onDialogCancel" />
      </q-card-section>
      <q-separator />
      <q-form ref="formRef" greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>User Info</legend>
              <div class="row justify-end q-mb-md">
                <q-btn
                  icon="o_add"
                  label="Add Role"
                  color="primary"
                  no-caps
                  @click="addRole"
                />
              </div>
              <q-separator />
              <div v-for="(role, roleIndex) in roles" :key="role.id" class="q-mt-md">
                <div class="row q-col-gutter-md items-start q-mb-sm">
                  <!-- Role Name -->
                  <div class="col-4">

                    <div class="q-mb-xs text-black">
                      Role Name<span class="required">*</span>
                    </div>

                    <q-input
                      v-model="role.roleName"
                      outlined
                      dense
                      :rules="[val => !!val || 'Role name is required']"
                    />
                  </div>
                  <!-- Users Table -->
                  <div class="col-7">
                    <q-markup-table flat bordered dense separator="cell">
                      <thead class="bg-primary text-white">
                        <tr>
                          <th class="text-left">Username</th>
                          <th class="text-left">Password</th>
                          <th class="text-center" style="width:80px">Action</th>
                        </tr>
                      </thead>
                      <tbody>
                        <tr
                          v-for="(user, userIndex) in role.users"
                          :key="user.id"
                        >
                          <td>
                            <q-input
                              v-model="user.userName"
                              outlined
                              dense
                              :rules="[val => !!val || 'Username is required']"
                            />
                          </td>

                          <td>
                            <q-input
                              v-model="user.password"
                              :type="user.showPassword ? 'text' : 'password'"
                              outlined
                              dense
                              :rules="[val => !!val || 'Password is required']"
                            >
                              <template #append>
                                <q-icon
                                  :name="user.showPassword ? 'o_visibility_off' : 'o_visibility'"
                                  class="cursor-pointer"
                                  @click="user.showPassword = !user.showPassword"
                                />
                              </template>
                            </q-input>
                          </td>

                          <td class="text-center">
                            <q-btn
                              icon="o_delete"
                              color="negative"
                              flat
                              dense
                              @click="removeUser(roleIndex, userIndex)"
                            />
                          </td>
                        </tr>

                        <!-- Add user row -->
                        <tr>
                          <td colspan="3">
                            <q-btn
                              label="Add User"
                              icon="o_add"
                              flat
                              outline
                              color="primary"
                              @click="addUser(roleIndex)"
                            />
                          </td>
                        </tr>

                      </tbody>

                    </q-markup-table>
                  </div>
                  <div class="col-1 flex justify-center q-pt-lg">

                    <q-btn
                      icon="o_delete"
                      color="negative"
                      flat
                      dense
                      @click="removeRole(roleIndex)"
                    >
                      <q-tooltip>Delete Role</q-tooltip>
                    </q-btn>

                  </div>
                </div>
                <q-separator v-if="roleIndex !== roles.length - 1" />
              </div>
            </fieldset>
          </div>
        </div>
        <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
          <q-btn
            color="grey-4"
            push
            outline
            label="Close"
            type="button"
            class="text-grey-9 actionBtn"
            no-caps
            @click="onDialogCancel"
          />
          <q-btn
            color="primary"
            push
            outline
            label="Save"
            type="submit"
            class="actionBtn"
            :loading="processing"
            no-caps
          />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>
<script setup>
import { onMounted, ref } from "vue";
import { uid, useDialogPluginComponent } from "quasar";
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import infraProjectInstanceService from "../infraProjectInstance.service";

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });
const { dialogRef, onDialogOK, onDialogCancel } = useDialogPluginComponent();

const formRef = ref(null);
const processing = ref(false);
const deletedRoles = ref([]);
const deletedUsers = ref([]);

const roles = ref([
  {
    id: uid(),
    roleName: "",
    users: [
      {
        id: uid(),
        userName: "",
        password: ""
      }
    ]
  }
]);

const getInfraProjectInstanceRoleInDetailByInstanceId = async () => {
  try {
    const resp = await infraProjectInstanceService.getInfraProjectInstanceRoleInDetailByInstanceId(props.id);
    roles.value = resp.map(role => ({
      ...role,
      flag: "Edit",
      users: role.infraProjectInstanceRoleUsers.map(user => ({
        ...user,
        flag: "Edit",
        showPassword: false
      }))
    }));
  } catch (error) {
    console.error("Failed to load Infra Project Instance:", error);
  }
};

function removeUser (roleIndex, userIndex) {
  const users = roles.value[roleIndex].users;
  const user = users[userIndex];

  if (users.length === 1) {
    notifyError({ message: "At least one user is required." });
    return;
  }

  if (user.flag === "New") {
    users.splice(userIndex, 1);
    return;
  }

  zwConfirmDelete(
    { data: `${user.userName}` },
    () => {
      if (user.flag !== "New") {
        user.flag = "Delete";
        deletedUsers.value.push({
          ...user,
          roleId: roles.value[roleIndex].id
        });
      }
      users.splice(userIndex, 1);
    },
    () => {}
  );
}

function removeRole (roleIndex) {
  const role = roles.value[roleIndex];

  if (roles.value.length === 1) {
    notifyError({ message: "At least one role is required." });
    return;
  }

  if (role.flag === "New") {
    roles.value.splice(roleIndex, 1);
    return;
  }

  zwConfirmDelete(
    { data: `${role.roleName}` },
    () => {
      if (role.flag !== "New") {
        role.flag = "Delete";
        deletedRoles.value.push(role);
      }
      roles.value.splice(roleIndex, 1);
    },
    () => {}
  );
}

function addRole () {
  roles.value.push({
    id: uid(),
    roleName: "",
    flag: "New",
    users: [
      {
        id: uid(),
        userName: "",
        password: "",
        flag: "New",
        showPassword: false
      }
    ]
  });
}

function addUser (roleIndex) {
  roles.value[roleIndex].users.push({
    id: uid(),
    userName: "",
    password: "",
    flag: "New",
    showPassword: false
  });

  formRef.value.resetValidation();
}

async function onSubmit () {
  // Check at least one role
  if (!roles.value || roles.value.length === 0) {
    notifyError({ message: "Add at least one role." });
    return;
  }

  // Check at least one user inside roles
  const hasUser = roles.value.some(role => role.users && role.users.length > 0);

  if (!hasUser) {
    notifyError({ message: "Add at least one user." });
    return;
  }

  const valid = await formRef.value.validate();

  if (!valid) {
    notifyError({ message: "Please fill all required fields." });
    return;
  }

  const payload = {
    projectInstanceId: props.id,
    infraProjectInstanceRoleList: [
      ...roles.value.map(role => ({
        id: role.id,
        roleName: role.roleName,
        flag: role.flag,
        infraProjectInstanceRoleUserList: [
          ...role.users.map(user => ({
            id: user.id,
            userName: user.userName,
            password: user.password,
            flag: user.flag
          })),
          ...deletedUsers.value
        ]
      })),
      ...deletedRoles.value.map(role => ({
        id: role.id,
        roleName: role.roleName,
        flag: "Delete",
        infraProjectInstanceRoleUserList: []
      }))
    ]
  };

  processing.value = true;

  try {
    await infraProjectInstanceService.saveInfraProjectInstanceRoles(payload);

    notifySuccess({
      message: "Role and Users saved successfully."
    });

    onDialogOK();
  } finally {
    processing.value = false;
  }
}

// On page rendering
onMounted(async () => {
  await getInfraProjectInstanceRoleInDetailByInstanceId();
  if (!roles.value || roles.value.length === 0) {
    roles.value.push({
      id: uid(),
      roleName: "",
      flag: "New",
      users: [
        {
          id: uid(),
          userName: "",
          password: "",
          flag: "New",
          showPassword: false
        }
      ]
    });
  }
});
</script>

<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
</style>
