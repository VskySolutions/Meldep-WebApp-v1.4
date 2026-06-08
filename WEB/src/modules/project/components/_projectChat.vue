<template>
  <q-dialog ref="chatContainer" class="project-message-dialog customDialog dialog-scrollable-content" persistent position="right">
    <q-card style="width: 550px; max-width: 90vw; height: 70%;">
      <!-- <q-bar class="bg-primary text-white">
        <div class="q-space">Project Chat</div>
        <q-btn dense flat icon="o_close" @click="toggleProjectChatBox" />
      </q-bar> -->

      <q-card-section class="card-header with-tools bg-primary stickyHeader flex justify-between items-center">
        <div class="text-h2 text-white">Project Chat</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>

      <q-card-section class="q-pa-lg message-card row items-end" style="height: 85%; overflow-y: auto;padding:0px 40px !important;">
        <div class="col-12">
          <q-chat-message
            v-for="(msg, index) in messages"
            :key="index"
            :text="[msg.message]"
            :sent="msg.isSent"
            :bg-color="msg.isSent ? '#f3f3f3' : 'primary'"
            :text-color="msg.isSent ? 'black' : 'white'"
          >
            <template #name>
              <span v-if="msg.isSent">You</span>
              <span v-else><i>From {{ msg.sentByUser.person.fullName }}</i></span>
            </template>
            <template #stamp>
              <div>
                {{ msg.formattedDate }}
              </div>
              <div class="vertical-messag-button">
                <q-btn flat dense round color="primary" icon="o_more_vert">
                  <q-menu>
                    <q-list style="min-width: 40px">
                      <q-item v-close-popup clickable class="q-pa-none" @click="action1">
                        <q-item-section><q-btn dense flat icon="o_sentiment_satisfied" @click="toggleReaction(index)">
                          <q-tooltip>Reaction</q-tooltip>
                        </q-btn></q-item-section>
                      </q-item>
                      <q-item v-if="msg.isSent" v-close-popup clickable @click="action2">
                        <q-item-section>
                          <q-btn dense flat icon="o_delete" @click="onDelete(index, msg.id, msg.message)">
                            <q-tooltip>Delete</q-tooltip>
                          </q-btn>
                        </q-item-section>
                      </q-item>
                    </q-list>
                  </q-menu>
                </q-btn>
              </div>
              <div class="">
                <q-menu v-model="reactionsVisible[index]">
                  <q-list dense>
                    <q-item
                      v-for="(reaction, rIndex) in reactions"
                      :key="rIndex"
                      clickable
                      @click="addReaction(index, msg.id, reaction.value, msg.message)"
                    >
                      <q-item-section> <div v-html="reaction.value" /></q-item-section>
                    </q-item>
                  </q-list>
                </q-menu>
                <div v-if="msg.reaction" class="reaction" v-html="msg.reaction" />
              </div>
            </template>
            <!-- Editable input on message click -->
            <template v-if="msg.isEditing && msg.isSent">
              <q-input
                v-model="msg.message"
                :dense="true"
                autofocus
                outlined stack-label hide-bottom-space
                @keyup="onKeyUpMessage"
                @keyup.enter="saveMessage(index, msg.id)"
                @blur="saveMessage(index, msg.id)"
              />
            </template>
            <!-- Show message as text when not editing -->
            <template v-else>
              <div class="editable-message" @click="startEditing(index, msg.id)">{{ msg.message }}</div>
            </template>
          </q-chat-message>
          <!-- Message input and send button -->
          <div class="row items-center">
            <div class="col-12">
              <!-- equivalent -->
              <q-input v-model="newMessage" label="Type a message..." filled @keyup.enter="sendMessage" @keyup="onKeyUpMessage">
                <template v-if="newMessage" #append>
                  <q-btn round dense flat icon="o_send" @click="sendMessage" />
                </template>
              </q-input>

              <q-list v-if="showSuggestions" class="suggestions">
                <q-item v-for="(person, index) in filteredPeople" :key="index" clickable @click="addMention(person.fullName, person.id)">
                  <q-item-section class="text-primary">{{ person.fullName }}</q-item-section>
                </q-item>
              </q-list>
            </div>
          </div>
        </div>
      </q-card-section>
    </q-card>
  </q-dialog>
</template>
<script setup>
import { ref, onMounted, nextTick } from "vue";
import { zwConfirmDelete } from "assets/utils";
import projectService from "modules/project/projects.service";

const loading = ref(true);

// Project ChatBox
// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });
const projectId = props.id;
// const chatOpen = ref(false);
const EmployeeIds = [];
const newMessage = ref("");
const messages = ref([]);
const messageModel = ref({
  message: "",
  projectId,
  employeeIds: []
});
// const toggleProjectChatBox = () => { chatOpen.value = !chatOpen.value; };
const peopleList = ref([]);
const showSuggestions = ref(false);
const filteredPeople = ref([]);
let mentionStart = -1;

// Get/Map project module list to table
const getProjectMessages = () => {
  projectService.getProjectMessages(projectId).then((resp) => {
    messages.value = Array.isArray(resp)
      ? resp.map((msg) => ({ ...msg, formattedDate: formatDateTime(msg.createdOnUtc) })) : [];
  }).finally(() => {
    loading.value = false;
  });
};

const sendMessage = () => {
  if (newMessage.value.trim() !== "") {
    messageModel.value.message = newMessage.value;
    messageModel.value.projectId = projectId;
    messageModel.value.employeeIds = EmployeeIds;
    projectService.sentMessage(messageModel.value).then((resp) => {
      // Ensure messages is reactive and array
      messages.value = [...messages.value, {
        id: resp.id,
        message: newMessage.value,
        isSent: true,
        isEditing: false,
        time: "",
        reaction: null
      }];
      newMessage.value = "";
      getProjectMessages();
      scrollToBottom();
    }).finally((resp) => {
    });
  }
};

function formatDateTime (dateStr) {
  const options = { weekday: "long", hour: "2-digit", minute: "2-digit", hour12: true };
  return new Date(dateStr).toLocaleString(undefined, options);
}

const scrollToBottom = () => {
  nextTick(() => {
    const container = document.querySelector(".message-card");
    if (container) {
      container.scrollTop = container.scrollHeight;
    }
  });
};

const onKeyUpMessage = (event) => {
  const cursorPos = event.target.selectionStart;
  const lastAtPos = newMessage.value.lastIndexOf("@", cursorPos - 1);
  if (lastAtPos !== -1 && cursorPos >= lastAtPos + 1) {
    mentionStart = lastAtPos;
    const searchTerm = newMessage.value.slice(lastAtPos + 1, cursorPos).toLowerCase();
    filteredPeople.value = peopleList.value.filter((person) =>
      person.fullName.toLowerCase().includes(searchTerm)
    );
    showSuggestions.value = filteredPeople.value.length > 0;
  } else {
    showSuggestions.value = false;
  }
};

const addMention = (person, id) => {
  const beforeMention = newMessage.value.slice(0, mentionStart);
  const afterMention = newMessage.value.slice(mentionStart).replace(/@\S*/, ""); // This removes the partial mention text

  // Insert the mention and keep the remaining text intact
  newMessage.value = `${beforeMention}@${person} ${afterMention}`.trim();
  EmployeeIds.push(id);
  showSuggestions.value = false;
  mentionStart = -1;
};

// Emoji Reactions Setup
const reactionsVisible = ref([]);
const reactions = [
  { label: "&#128512; Happy", value: "&#128512;" }, // 😀
  { label: "&#128514; Laugh", value: "&#128514;" }, // 😂
  { label: "&#128545; Angry", value: "&#128545;" }, // 😡
  { label: "&#128532; Sad", value: "&#128532;" }, // 😔
  { label: "&#128525; Love", value: "&#128525;" } // 😍
];

// Toggle reaction menu
const toggleReaction = (index) => {
  reactionsVisible.value = [];
  reactionsVisible.value[index] = true;
};

// Add selected reaction to message
const addReaction = (index, id, reaction, msg) => {
  reactionsVisible.value[index] = false;
  messageModel.value.reaction = reaction;
  messageModel.value.message = msg;
  messageModel.value.projectId = projectId;
  projectService.updateMessage(id, messageModel.value).then((resp) => {
  }).finally(() => {
    messages.value[index].reaction = reaction;
  });
};

function startEditing (index, id) {
  messages.value[index].isEditing = true;
}

function saveMessage (index, id) {
  messageModel.value.message = messages.value[index].message;
  messageModel.value.projectId = projectId;
  messageModel.value.employeeIds = EmployeeIds;
  projectService.updateMessage(id, messageModel.value).then((resp) => {
  }).finally(() => {
    messages.value[index].isEditing = false;
  });
}

// Delete Message
const onDelete = (index, id, msg) => {
  zwConfirmDelete({ data: `${msg}` }, () => {
    projectService.deleteMessage(id).then(resp => {
      messages.value.splice(index, 1);
    });
  });
};

const getProjectEmployees = (projectId) => {
  projectService.getProjectEmployees(projectId).then((resp) => {
    peopleList.value = resp.map((e) => ({
      id: e.value,
      fullName: e.text
    }));
  }).finally(() => {
  });
};

onMounted(() => {
  getProjectMessages(projectId);
  getProjectEmployees(projectId);
});
</script>
<style>
.suggestions{
  height: 200px;
  overflow-y: scroll;
}
.project-message-dialog .q-message-text{
  position: relative;
}
.vertical-messag-button{
  position: absolute;
  top:0px;
  right: -25px;
}
.reaction{
  position:absolute;
  right: 0px;
}
</style>
