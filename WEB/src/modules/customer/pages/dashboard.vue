<template>
  <q-page padding>
    <q-card class="breadcrumSection project6 flex justify-between items-center">
      <!-- Breadcrumb Section -->
      <q-card-section class="card-header with-tools flex justify-between items-center">
        <q-breadcrumbs class="text-brown text-weight-bold text-h3">
          <template v-slot:separator>
            <q-icon size="1.5em" name="o_chevron_right" color="primary" />
          </template>
          <q-breadcrumbs-el label="Customers" clickable to="/customer"/>
          <q-breadcrumbs-el :label="'Customer Center - ' + (model.name)"/>
        </q-breadcrumbs>
      </q-card-section>
      <!-- Chat Popup Button -->
      <div>
        <q-btn round color="primary" @click="toggleChat" class="fixed-button q-mr-sm hidden"> <i class="fa-brands fa-facebook-messenger"></i><q-tooltip>Message</q-tooltip></q-btn>
        <q-btn icon="o_chevron_left" outline label="Back To List" no-caps class="text-primary btnRounded q-mr-lg no-space-between" @click="$router.push('/customer')" />
      </div>
    </q-card>

    <div>
      <!-- Chat Popup Window -->
      <q-dialog v-model="chatOpen" ref="chatContainer" class="project-message-dialog customDialog dialog-scrollable-content" persistent position="right" >
        <q-card style="width: 550px; max-width: 90vw; height: 60%;">
          <q-bar class="bg-primary text-white">
            <div class="q-space">{{ model.name }}</div>
            <q-btn dense flat icon="o_close" @click="toggleChat" />
          </q-bar>

          <q-card-section class="q-pa-lg message-card row items-end" style="height: 90%; overflow-y: auto;padding:0px 40px !important;">
            <div class="col-12">
              <q-chat-message
                v-for="(msg, index) in messages"
                :key="index"
                :text="[msg.message]"
                :sent="msg.isSent"
                :bg-color="msg.isSent ? '#f3f3f3' : 'primary'"
                :text-color="msg.isSent ? 'black' : 'white'"
              >
                <template v-slot:name>
                  <span v-if="msg.isSent">You</span>
                  <span v-else><i>From {{ msg.sentByUser.person.fullName }}</i></span>
                </template>
                <template v-slot:stamp>
                  <div>
                    {{ msg.formattedDate }}
                  </div>
                  <div class="vertical-messag-button">
                    <q-btn flat dense round color="primary" icon="o_more_vert">
                      <q-menu>
                        <q-list style="min-width: 40px">
                          <q-item clickable v-close-popup @click="action1" class="q-pa-none">
                            <q-item-section><q-btn dense flat icon="o_sentiment_satisfied" @click="toggleReaction(index)">
                              <q-tooltip>Reaction</q-tooltip>
                            </q-btn></q-item-section>
                          </q-item>
                          <q-item v-if="msg.isSent" clickable v-close-popup @click="action2">
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
                          <q-item-section> <div v-html="reaction.value"></div></q-item-section>
                        </q-item>
                      </q-list>
                    </q-menu>
                    <div v-if="msg.reaction" class="reaction" v-html="msg.reaction"></div>
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
                  <div @click="startEditing(index, msg.id)" class="editable-message">{{ msg.message }}</div>
                </template>
              </q-chat-message>
              <!-- Message input and send button -->
              <div class="row items-center">
                <div class="col-12">
                  <!-- equivalent -->
                  <q-input v-model="newMessage" label="Type a message..." @keyup.enter="sendMessage" @keyup="onKeyUpMessage" filled>
                    <template v-if="newMessage" v-slot:append>
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
    </div>

    <div class="items-center">
      <div class="q-pt-md cardTable">
        <div class="q-gutter-y-md">
          <q-card class="PersonMain card-header with-tools headerBasic">
            <q-tabs v-model="tab" dense class="text-primary" active-color="primary" indicator-color="primary" active-class="bg-blue-1 borderRadiusTabs" align="left" narrow-indicator>
              <q-tab name="1_tab" :label="model.person ? 'Customer Info' : 'Company Info'" class="q-px-lg q-mr-md" />
              <q-tab v-if="!model.person" name="2_tab" label="Contact Details" class="q-px-lg" />
              <q-tab name="3_tab" label="Projects" class="q-px-lg" />
              <q-tab name="4_tab" :label="model.person ? 'Customer Files' : 'Company Files'" class="q-px-lg" />
              <q-tab name="5_tab" label="Notes" class="q-px-lg" />
            </q-tabs>
            <q-tab-panels v-model="tab" animated>

              <q-tab-panel name="1_tab">
                <customerProfileTab :customerId="customerId" :isPerson="model.person ? true : false"/>
              </q-tab-panel>

              <q-tab-panel name="2_tab">
                <customerContactDetailsTab :companyId="companyId" :isPerson="model.person ? true : false"/>
              </q-tab-panel>

              <q-tab-panel name="3_tab" class="items-center q-pa-md q-mx-auto">
                <customerProjectsTab :customerId="customerId" :isPerson="model.person ? true : false"/>
              </q-tab-panel>

              <q-tab-panel name="4_tab" class="items-center q-pa-md q-mx-auto">
                <customerFilesTab :customerId="customerId" :isPerson="model.person ? true : false"/>
              </q-tab-panel>

              <q-tab-panel name="5_tab" class="items-center q-pa-md q-mx-auto">
                <customerNotesTab :customerId="customerId" :isPerson="model.person ? true : false"/>
              </q-tab-panel>

            </q-tab-panels>
          </q-card>
        </div>
      </div>
    </div>
  </q-page>
</template>

<script setup>
import { useDialogPluginComponent } from "quasar";
import { onMounted, ref } from "vue";
import _ from "lodash";
// import { useRoute } from "vue-router";
import customerService from "modules/customer/customer.service";

// Tabs
import customerProfileTab from "../components/_customerProfileTab.vue";
import customerContactDetailsTab from "../components/_customerContactDetailsTab.vue";
import customerProjectsTab from "../components/_customerProjectsTab.vue";
import customerFilesTab from "../components/_customerFilesTab.vue";
import customerNotesTab from "../components/_customerNotesTab.vue";

// const route = useRoute();
const customerId = history.state?.customerId;
const companyId = history.state?.companyId;

defineEmits([...useDialogPluginComponent.emits]);
const tab = ref("1_tab");

const model = ref({
  description: ""
});

// const customerIds = history.state?.customerId ? customerId : "";
// Search variables
const loading = ref(true);

const getCompanyDetails = () => {
  loading.value = true;
  customerService.getCustomerDetails(customerId).then((resp) => {
    model.value = _.cloneDeep(resp);
    if (resp.company) {
      model.value.name = resp.company.name;
    }
    // Set person details
    if (resp.person) {
      model.value.name = resp.person.firstName + " " + resp.person.lastName || "";
    }
  }).finally(() => {
    loading.value = false;
  });
};

onMounted(() => {
  getCompanyDetails();
});
</script>
