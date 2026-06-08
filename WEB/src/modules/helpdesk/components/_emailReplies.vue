<template>
  <div class="q-pa-md row justify-center">
    <div class="col-grow email-thread">
      <div class="email-layout">
        <!-- REPLY BOX -->
        <q-slide-transition>
          <q-card v-if="showReply" class="email-editor-sticky">
            <div class="reply-box q-pa-sm" flat bordered>
              <!-- FROM / TO (READONLY) -->
              <div class="q-px-sm q-pt-sm q-mb-sm text-caption text-grey-8">
                <div class="q-mb-xs">
                  <b>From:</b>
                  <span class="text-weight-bold text-black">
                    {{ loggedUserName }}
                  </span>
                  ({{ loggedInEmails }})
                </div>
                <div>
                  <b>To: </b>
                  <span class="text-weight-bold text-black">
                    {{ replyToEmail }}
                  </span>
                </div>
                <!-- <div class="row">
                  <div class="q-mt-sm col-4">
                    <label>Additional To</label>
                    <multiSelectDropdown
                      v-model="selectedToEmails"
                      :options="personPrimaryEmailAddressDropdown.list.value"
                      :filter="personPrimaryEmailAddressDropdown.filter"
                      option-label="label"
                      option-value="value"
                      container-class="full-width"
                      input-class="col-12"
                    />
                  </div>
                  <div class="col-4"></div>
                  <div class="col-4"></div>
                  <div class="q-mt-sm col-4">
                    <label>CC</label>
                    <multiSelectDropdown
                      v-model="selectedCcEmails"
                      :options="personPrimaryEmailAddressDropdown.list.value"
                      :filter="personPrimaryEmailAddressDropdown.filter"
                      option-label="label"
                      option-value="value"
                      container-class="full-width"
                      input-class="col-12"
                    />
                  </div>
                </div> -->
                <div class="column">
                  <!-- Additional To -->
                  <div class="row items-center">
                    <div class="">
                      <label class="text-weight-medium">External To</label>
                    </div>

                    <div class="col-3">
                      <multiSelectDropdown
                        v-model="selectedToEmails"
                        :options="personPrimaryEmailAddressDropdown.list.value"
                        :filter="personPrimaryEmailAddressDropdown.filter"
                        option-label="label"
                        option-value="value"
                        container-class="full-width"
                        input-class="col-12"
                      />
                    </div>
                  </div>

                  <!-- CC -->
                  <div class="row items-center">
                    <div class="">
                      <label class="text-weight-medium">CC</label>
                    </div>

                    <div class="col-3">
                      <multiSelectDropdown
                        v-model="selectedCcEmails"
                        :options="personPrimaryEmailAddressDropdown.list.value"
                        :filter="personPrimaryEmailAddressDropdown.filter"
                        option-label="label"
                        option-value="value"
                        container-class="full-width"
                        input-class="col-12"
                      />
                    </div>
                  </div>

                </div>
              </div>
              <q-separator />
              <!-- EDITOR (NO TOOLBAR) -->
              <q-editor
                ref="editorRef"
                v-model="newEmail"
                flat
                class="reply-editor"
                placeholder="Type your reply..."
                :disable="helpDeskStatus === 'Completed' || helpDeskStatus === 'Closed' || helpDeskStatus === 'Cancelled'"
                :dense="$q.screen.lt.md"
                :toolbar="[]"
              />
              <div class="editor-toolbar-bottom row items-center q-mb-sm">
                <q-separator vertical />
                <!-- TEXT STYLE -->
                <q-btn dense flat icon="o_format_bold" @click="cmd('bold')" />
                <q-btn dense flat icon="o_format_italic" @click="cmd('italic')" />
                <q-btn dense flat icon="o_format_strikethrough" @click="cmd('strikeThrough')" />
                <q-btn dense flat icon="o_format_underlined" @click="cmd('underline')" />
                <q-separator vertical />
                <!-- LINK / HR -->
                <q-btn dense flat icon="o_link" @click="cmd('link')" />
                <q-btn dense flat icon="o_horizontal_rule" @click="cmd('insertHorizontalRule')" />
                <q-separator vertical />
                <!-- FORMAT DROPDOWN -->
                <q-select
                  v-model="formatBlock"
                  dense
                  outlined
                  emit-value
                  map-options
                  style="width: 130px"
                  :options="formatOptions"
                  @update:model-value="setFormat"
                />
                <q-btn dense flat icon="o_format_clear" @click="cmd('removeFormat')" />
                <q-separator vertical />
                <!-- LIST / INDENT -->
                <q-btn dense flat icon="o_format_list_bulleted" @click="cmd('insertUnorderedList')" />
                <q-btn dense flat icon="o_format_list_numbered" @click="cmd('insertOrderedList')" />
                <q-separator vertical />
              </div>
              <q-separator />
              <!-- FOOTER -->
              <div class="row justify-end q-px-sm q-mt-xs">
                <span class="text-grey-6 fs-12"> Note: Add a detailed ticket description here and attach screenshots if needed</span>
                <q-space />
                <q-btn
                  label="Send"
                  color="primary"
                  icon="o_send"
                  :loading="processing"
                  :disable="processing || helpDeskStatus === 'Completed' || helpDeskStatus === 'Closed' || helpDeskStatus === 'Cancelled'"
                  @click="sendEmail"
                />
              </div>
            </div>
          </q-card>
        </q-slide-transition>
        <!-- EMAIL THREAD -->
        <div class="email-thread-scroll" :class="{ 'q-pt-md': showReply }">
          <q-inner-loading :showing="loading" class="dark-loader">
            <q-spinner-ios size="40px" color="primary" />
          </q-inner-loading>
          <div
            v-if="!loading && emailRepliesList.length === 0"
            class="q-pa-sm text-center text-red"
          >
            No user replies yet. Tick ‘Show System Emails’ to view system emails
          </div>
          <div v-else>
            <div v-for="(group, date) in emailRepliesRows" :key="date">
              <div
                v-for="reply in group"
                :key="reply.id"
                class="email-message"
                :class="reply.fromEmail === loggedInEmails
                  ? 'email-outgoing'
                  : 'email-incoming'"
              >
                <q-card flat bordered class="q-pa-md email-card">
                  <!-- From -->
                  <div class="row items-center justify-between q-mb-sm">
                    <div class="">
                      <span class="text-caption text-grey-8 text-weight-bold q-mb-xs">From:</span>
                      <span class="q-ml-xs text-primary text-weight-bold">
                        <!-- {{ reply.fromEmail === loggedInEmails ? loggedUserName + " " + "(" + loggedInEmails + ")"   : reply.fromEmail }} -->
                        {{ reply.fromName ? reply.fromName + ' (' + reply.fromEmail + ')' : reply.fromEmail }}
                      </span>
                      <span class="text-black q-ml-xs">
                        {{ reply.twilioStatus }} • {{ reply.createdOnUtc }}
                      </span>
                    </div>
                  </div>
                  <!--To -->
                  <div class="row items-center justify-between">
                    <div>
                      <span class="text-caption text-grey-8 q-mb-xs text-weight-bold">To: </span>
                      <span class="text-caption text-grey-8 q-mb-xs text-weight-bold">
                        <!-- {{ reply.toEmail }} -->
                        <!-- {{ reply.toName ? reply.toName + ' (' + reply.toEmail + ')' : reply.toEmail }} -->
                        {{ formatEmails(reply.toName, reply.toEmail).slice(0, 2).join(', ') }}
                        <!-- Show +N if more than 2 -->
                        <span
                          v-if="formatEmails(reply.toName, reply.toEmail).length > 2 && !showAllToEmails"
                          class="cursor-pointer text-primary"
                          @click="showAllToEmails = true"
                        >
                          +{{ formatEmails(reply.toName, reply.toEmail).length - 2 }}
                          <q-tooltip>View More</q-tooltip>
                        </span>
                      </span>

                      <!-- Remaining emails when expanded -->
                      <div
                        v-if="formatEmails(reply.toName, reply.toEmail).length > 2 && showAllToEmails"
                        class="q-mt-xs text-caption text-grey-8 text-weight-bold"
                      >
                        {{ formatEmails(reply.toName, reply.toEmail).slice(2).join(', ') }}
                        <span
                          class="cursor-pointer text-primary"
                          @click="showAllToEmails = false"
                        >
                          View Less
                        </span>
                      </div>
                      <span class="text-black q-ml-xs">
                        <!-- {{ reply.twilioStatus }} • {{ reply.createdOnUtc }} -->
                      </span>
                    </div>
                  </div>
                  <!--External To Emails-->
                  <div v-if="reply.externalToEmail" class="row items-center justify-between">
                    <div>
                      <span class="text-caption text-grey-8 q-mb-xs text-weight-bold">External To: </span>
                      <span class="text-caption text-grey-8 q-mb-xs text-weight-bold">
                        {{ formatEmails(reply.externalName, reply.externalToEmail).slice(0, 2).join(', ') }}
                        <!-- Show +N if more than 2 -->
                        <span
                          v-if="formatEmails(reply.externalName, reply.externalToEmail).length > 2 && !showAllExternalToEmails"
                          class="cursor-pointer text-primary"
                          @click="showAllExternalToEmails = true"
                        >
                          +{{ formatEmails(reply.externalName, reply.externalToEmail).length - 2 }}
                          <q-tooltip>View More</q-tooltip>
                        </span>
                      </span>

                      <!-- Remaining emails when expanded -->
                      <div
                        v-if="formatEmails(reply.externalName, reply.externalToEmail).length > 2 && showAllExternalToEmails"
                        class="q-mt-xs text-caption text-grey-8 text-weight-bold"
                      >
                        {{ formatEmails(reply.externalName, reply.externalToEmail).slice(2).join(', ') }}
                        <span
                          class="cursor-pointer text-primary"
                          @click="showAllExternalToEmails = false"
                        >
                          View Less
                        </span>
                      </div>
                    </div>
                  </div>
                  <!--CC -->
                  <div v-if="reply.ccEmail" class="row items-center justify-between q-mb-sm">
                    <div>
                      <span class="text-caption text-grey-8 q-mb-xs text-weight-bold">CC: </span>
                      <span class="text-caption text-grey-8 q-mb-xs text-weight-bold">
                        {{ formatEmails(reply.ccName, reply.ccEmail).slice(0, 2).join(', ') }}
                        <!-- Show +N if more than 2 -->
                        <span
                          v-if="formatEmails(reply.ccName, reply.ccEmail).length > 2 && !showAllCCEmails"
                          class="cursor-pointer text-primary"
                          @click="showAllCCEmails = true"
                        >
                          +{{ formatEmails(reply.ccName, reply.ccEmail).length - 2 }}
                          <q-tooltip>View More</q-tooltip>
                        </span>
                      </span>

                      <!-- Remaining emails when expanded -->
                      <div
                        v-if="formatEmails(reply.ccName, reply.ccEmail).length > 2 && showAllCCEmails"
                        class="q-mt-xs text-caption text-grey-8 text-weight-bold"
                      >
                        {{ formatEmails(reply.ccName, reply.ccEmail).slice(2).join(', ') }}
                        <span
                          class="cursor-pointer text-primary"
                          @click="showAllCCEmails = false"
                        >
                          View Less
                        </span>
                      </div>
                    </div>
                  </div>
                  <!--Subject -->
                  <div class="row items-center justify-between q-mt-sm q-mb-sm">
                    <span class="text-caption text-black q-mb-xs text-weight-bold">Subject:
                      {{ reply.subject }}
                    </span>
                  </div>
                  <!-- Body -->
                  <div class="q-mb-sm">
                    <span class="text-caption text-black text-weight-bold q-mb-xs">
                      Body:
                    </span>
                    <div class="email-body text-black q-mt-xs RichTextEditor" v-html="reply.body" />
                  </div>
                </q-card>
              </div>
            </div>
          </div>
        </div>
        <div
          v-if="!disableLoadMore && !loading"
          class="text-center q-pa-sm"
        >
          <q-btn
            icon-right="o_refresh"
            color="secondary"
            outlined
            size="sm"
            @click="getAllHelpDeskEmailRepliesMappingList(skipIndex)"
          >
            Load More
          </q-btn>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useQuasar } from "quasar";
import { ref, onMounted, computed, watch } from "vue";
import helpDeskService from "modules/helpdesk/helpDesk.service";
import { useAuthStore } from "stores/auth";
import { notifySuccess, notifyError, notifyWarning } from "assets/utils";

// Shared Dropdowns
import personModule from "src/modules/person/utils/dropdowns.js";

// Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// define emits
const emit = defineEmits(["update:modelValue"]);

// Props values i.e. come from query string
const props = defineProps({
  helpDeskId: { type: String, default: "" },
  title: { type: String, default: "" },
  employeeId: { type: String, default: "" },
  loggedInEmail: { type: String, default: "" },
  twilioEmailId: { type: String, default: "" },
  modelValue: { type: String, default: "" },
  showReply: { type: Boolean, default: false },
  defaultTab: {
    type: String,
    default: "1_tab"
  },
  showSystemEmails: { type: Boolean, default: false },
  notesType: { type: String, default: "" }
});

// common variables
const authStore = useAuthStore();
const user = authStore.user;
const loggedUserName = user ? user.firstName + " " + user.lastName : "";
const loggedInEmails = user ? user?.userEmail : "";
const $q = useQuasar();
const editorRef = ref(null);
const loading = ref(true);
const tab = ref("1_tab");
const helpDeskStatus = ref(null);
const replyToEmail = ref("");
const processing = ref(false);
const showAllToEmails = ref(false);
const showAllCCEmails = ref(false);
const showAllExternalToEmails = ref(false);
const take = ref(10);
const skipIndex = ref(0);
const disableLoadMore = ref(false);
const selectedToEmails = ref([]);
const selectedCcEmails = ref([]);

const newEmail = computed({
  get: () => props.modelValue,
  set: (val) => emit("update:modelValue", val)
});

const getStorageKey = (helpDeskId) => `helpdesk_email_pagination_${helpDeskId}`;

const savePaginationToStorage = () => {
  if (!props.helpDeskId) return;

  localStorage.setItem(
    getStorageKey(props.helpDeskId),
    JSON.stringify({
      loadedPages: skipIndex.value, // pages already loaded
      take: take.value,
      showSystemEmails: props.showSystemEmails
    })
  );
};

const restoreAndLoadEmails = async () => {
  emailRepliesList.value = [];
  disableLoadMore.value = false;

  // loadPaginationFromStorage();
  const hasStoredState = loadPaginationFromStorage();

  // If helpdesk was never opened before → load first page only
  if (!hasStoredState) {
    skipIndex.value = 0;
    await getAllHelpDeskEmailRepliesMappingList(0);
    return;
  }

  const pagesToLoad = skipIndex.value;

  skipIndex.value = 0; // reset before reloading

  for (let i = 0; i < pagesToLoad; i++) {
    await getAllHelpDeskEmailRepliesMappingList(i);
  }
};

const loadPaginationFromStorage = () => {
  // if (!props.helpDeskId) return;

  const raw = localStorage.getItem(getStorageKey(props.helpDeskId));
  if (!raw) return false;

  try {
    const data = JSON.parse(raw);
    skipIndex.value = data.loadedPages ?? 1;
    take.value = data.take ?? 10;
    return true;
  } catch {
    // skipIndex.value = 1;
    // take.value = 10;
    return false;
  }
};

// get and map email replies list
const emailRepliesList = ref([]);
const getAllHelpDeskEmailRepliesMappingList = async (index = skipIndex.value) => {
  loading.value = true;
  try {
    const resp =
      await helpDeskService.getAllHelpDeskEmailRepliesMappingList(
        props.helpDeskId,
        index,
        take.value,
        props.showSystemEmails
      );

    const list = resp.emailRepliesLists || [];
    emailRepliesList.value.push(...list);

    if (list.length < take.value) {
      disableLoadMore.value = true;
    }

    // pages loaded count
    skipIndex.value++;
    helpDeskStatus.value = list?.[0]?.statusText || null;
    replyToEmail.value = resp.replyToEmails?.join(", ");

    savePaginationToStorage();
  } finally {
    loading.value = false;
  }
};

// get email replies details
const emailRepliesRows = computed(() => {
  const groups = {};
  emailRepliesList?.value.forEach(reply => {
    // MM/dd/yyyy from "MM/dd/yyyy hh:mm tt"
    const date = reply.createdOnStr.split(" ")[0];

    if (!groups[date]) {
      groups[date] = [];
    }
    groups[date].push(reply);
  });

  return groups;
});

// email formats
const formatEmails = (toName, toEmail) => {
  if (!toEmail) return [];

  // Convert emails to array
  const emails = Array.isArray(toEmail)
    ? toEmail
    : toEmail.split(",").map(e => e.trim());

  return emails.map(email => {
    return toName
      ? `${toName} (${email})`
      : email;
  });
};

// remove multiple spaces, empty paragraphs and trim body
const sanitizeEditorHtml = (html) => {
  if (!html) return "";

  return html
    // remove multiple spaces
    .replace(/\s+/g, " ")
    // remove nbsp
    .replace(/&nbsp;/g, " ")
    // remove empty paragraphs
    .replace(/<p>\s*<\/p>/gi, "")
    .replace(/<p><br><\/p>/gi, "")
    // trim start/end
    .trim();
};

// save email replies
const sendEmail = async () => {
  //  Prevent double click
  if (processing.value) {
    notifyWarning({ message: "Double click not allowed. Please wait..." });
    return;
  }
  if (!newEmail.value || newEmail.value.trim() === "") {
    notifyWarning({ message: "Message cannot be empty." });
    return;
  }
  processing.value = true;
  try {
    if (newEmail.value.trim() !== "") {
      const payload = {
        helpDeskId: props.helpDeskId,
        body: sanitizeEditorHtml(newEmail.value),
        subject: props.title,
        twilioEmailId: props.twilioEmailId,
        externalToEmail: selectedToEmails.value.join(","),
        ccEmail: selectedCcEmails.value.join(",")
      };

      await helpDeskService.saveEmailReplies(payload);

      notifySuccess({ message: "Reply is sent successfully." });
      newEmail.value = "";
      selectedToEmails.value = null;
      selectedCcEmails.value = null;
      restoreAndLoadEmails();
    }
  } catch (error) {
    console.error("Error in submitting the reply:", error);
    notifyError({ message: "An error occurred while saving the reply." });
  } finally {
    processing.value = false;
    // setTimeout(() => {
    //   processing.value = false;
    // }, 6000);
  }
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Rich Editor Tools: Fonts and Toolbar
// --------------------------------------------------------------------------------------------------------------------------------------------------
const formatBlock = ref("p");
const formatOptions = [
  { label: "Formatting", value: "p" },
  { label: "H1", value: "h1" },
  { label: "H2", value: "h2" },
  { label: "H3", value: "h3" },
  { label: "H4", value: "h4" },
  { label: "H5", value: "h5" },
  { label: "H6", value: "h6" }
];

const cmd = (command, value = null) => {
  if (!editorRef.value) return;
  editorRef.value.runCmd(command, value);
};

const setFormat = (val) => {
  if (!editorRef.value) return;
  editorRef.value.runCmd("formatBlock", val);
};

// ------------------------------------------------------------------------------------
// All Dropdowns
// ------------------------------------------------------------------------------------

const { personPrimaryEmailAddressDropdown } = personModule();

watch(
  () => props.helpDeskId,
  (val) => {
    if (!val) return;
    restoreAndLoadEmails();
  },
  { immediate: true }
);

watch(
  () => props.showSystemEmails,
  async () => {
    restoreAndLoadEmails();
  }
);

// On page rendering
onMounted(() => {
  tab.value = props.defaultTab; // set active tab
  personPrimaryEmailAddressDropdown.load(user.siteId);

  console.log("personPrimaryEmailAddressDropdown", personPrimaryEmailAddressDropdown.list);
});

</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
.email-thread {
  display: flex;
  flex-direction: column;
}
.email-thread-scroll {
  position: relative;
  min-height: 300px;
}
.email-message {
  display: flex;
  margin-bottom: 10px;
}
.email-card {
  border-radius: 8px;
}
.email-message.email-incoming {
  justify-content: flex-start;
}
.email-message.email-outgoing {
  justify-content: flex-end;
}
.email-message.email-incoming .email-card {
  background: #fff8e1;
  width: 90% !important;
  word-wrap: break-word;
  overflow-wrap: break-word;
  white-space: normal;
}
.email-message.email-outgoing .email-card {
  background: #e6e6e7de;
  width: 90% !important;
  word-wrap: break-word;
  overflow-wrap: break-word;
  white-space: normal;
}
.email-body {
  white-space: normal;
  line-height: 1.6;
  font-size: 14px;
}
.email-body p {
  margin: 0;
}
.reply-box {
  border-radius: 8px;
  background: #ffffff;
}
.reply-editor {
  min-height: 180px;
  border: none;
}
.reply-editor {
  min-height: 180px;
  padding: 8px;
}
.editor-toolbar-bottom {
  background: #fafafa;
  flex-wrap: wrap;
}
.email-layout {
  display: flex;
  flex-direction: column;
}
.email-editor-sticky {
  border: 1px solid #e0e0e0;
  border-radius: 10px;
}
.email-body img {
  max-width: 100%;
  height: auto;
  display: block;
  margin: 8px 0;
}
.dark-loader {
  z-index: 10;
}
</style>
