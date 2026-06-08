<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center justify-between">
          <div class="col-12 col-md-4">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Help Desk" class="cursor-pointer" @click="$router.back()" />
              <q-breadcrumbs-el label="Workspace And Menu" />
            </q-breadcrumbs>
          </div>
          <q-btn icon="o_chevron_left" outline label="Back" no-caps class="text-primary btnRounded q-ml-sm" @click="$router.back()" />
        </div>
      </q-card-section>
      <q-separator />
      <div class="q-px-md">
        <div class="row q-col-gutter-sm">
          <q-inner-loading :showing="isLoading" color="primary">
            <div class="row items-center justify-center full-height">
              <q-spinner-ios size="40px" />
            </div>
          </q-inner-loading>
          <div class="col-12 col-md-6 q-mt-md">
            <div class="row items-center justify-between q-mb-md">
              <div class="text-h3">
                <span>Workspace</span>
              </div>
              <q-btn
                color="primary"
                icon="o_add"
                label="Add"
                no-caps
                @click="onAddHelpDeskTopic"
              />
            </div>
            <div
              v-for="(helpDeskTopic, helpDeskTopicIndex) in helpDeskTopics"
              :key="helpDeskTopic.id"
              :class="activeRowId === helpDeskTopic.id ? 'bg-grey-2' : ''"
              class="row items-center q-mb-md q-pa-sm"
              style="border: 1px solid #ccc;"
            >
              <div class="col cursor-pointer">
                <template v-if="helpDeskTopic.showInputForNewHelpDeskTopic || (activeEdit.field === 'title' && activeEdit.rowId === helpDeskTopic.id)">
                  <div class="row items-center">
                    <q-input
                      v-model="helpDeskTopic.title"
                      outlined
                      stack-label
                      hide-bottom-space
                      dense
                      class="col-6"
                      placeholder="Enter Workspace"
                      hint="Press Enter to save"
                      @keyup.enter="saveOrUpdateHelpDeskTopic(helpDeskTopic)"
                    >
                      <q-tooltip>
                        Press enter key to save Workspace
                      </q-tooltip>
                    </q-input>
                    <q-btn
                      v-if="helpDeskTopic.title && !helpDeskTopic.showInputForNewHelpDeskTopic"
                      icon="o_close"
                      size="xs"
                      color="black"
                      flat
                      round
                      dense
                      class="q-ml-sm"
                      @click="helpDeskTopic.title = helpDeskTopic._oldText;
                              activeEdit = { rowId: null, field: '' }"
                    />
                  </div>
                </template>

                <template v-else>
                  <div
                    class="row items-center justify-between q-pa-xs cursor-pointer"
                    @click="getAllHelpDeskTopicQuestionList(helpDeskTopic.title, helpDeskTopic.id)"
                    @dblclick="
                      helpDeskTopic._oldText = helpDeskTopic.title;
                      activeEdit = { rowId: helpDeskTopic.id, field: 'title' }
                    "
                  >
                    <span class="q-ml-xs">{{ helpDeskTopic.title }}</span>
                  </div>
                </template>

                <q-tooltip v-if="helpDeskTopic.title && !helpDeskTopic.showInputForNewHelpDeskTopic && activeEdit.rowId !== helpDeskTopic.id">Double-click to edit</q-tooltip>
              </div>
              <q-icon
                v-if="helpDeskTopic.title"
                name="o_notes"
                color="primary"
                size="sm"
                class="cursor-pointer q-mx-sm"
                @click="openDescriptionDialog(helpDeskTopic, null)"
              >
                <q-tooltip>Description</q-tooltip>
              </q-icon>
              <q-icon
                v-else
                name="o_notes"
                color="grey-5"
                size="sm"
                class="q-mx-sm"
                disable
              >
                <q-tooltip>Enter workspace first</q-tooltip>
              </q-icon>
              <q-icon
                v-if="hasValidDescription(helpDeskTopic.description)"
                name="o_info"
                size="16px"
                class="q-mx-sm"
              >
                <q-tooltip class="text-wrap break-words RichTextEditor" max-width="300px">
                  <div v-html="helpDeskTopic.description" />
                </q-tooltip>
              </q-icon>
              <q-toggle
                v-model="helpDeskTopic.isActive"
                dense class="q-mx-md"
                @update:model-value="() => saveOrUpdateHelpDeskTopic(helpDeskTopic)"
              />
              <q-icon
                name="o_delete"
                color="negative"
                size="sm"
                class="cursor-pointer q-ml-sm"
                @click="onDeleteHelpDeskTopic(helpDeskTopic, helpDeskTopicIndex)"
              >
                <q-tooltip>Delete</q-tooltip>
              </q-icon>
              <q-dialog v-model="isDescriptionDialogForTopic[helpDeskTopic.id]" persistent transition-show="scale" transition-hide="scale" @hide="onDialogHide">
                <q-card class="rightDialog">
                  <q-card-section class="card-header with-tools position-relative">
                    <div class="text-black text-h2">
                      Description For
                      <span class="text-primary">{{ helpDeskTopic.title }}</span>
                    </div>
                    <q-btn v-close-popup icon="o_close" class="close" flat round dense style="position: absolute; right: 5px; top: 10px;" />
                  </q-card-section>

                  <q-card-section>
                    <q-editor
                      v-model="helpDeskTopic.description"
                      :model-value="helpDeskTopic.description ?? ''"
                      :dense="$q.screen.lt.md"
                      :toolbar="toolbar"
                      :fonts="fonts"
                      placeholder="Type description here..."
                    />
                  </q-card-section>

                  <q-card-actions align="right">
                    <q-btn v-close-popup color="grey-8" flat dense bordered label="Close" />
                    <q-btn color="primary" flat bordered label="Save" @click="saveOrUpdateHelpDeskTopic(helpDeskTopic, helpDeskTopicIndex)" />
                  </q-card-actions>
                </q-card>
              </q-dialog>
            </div>
          </div>
          <div class="col-12 col-md-6 q-mt-md">
            <div class="row items-center justify-between q-mb-md">
              <div class="text-h3">
                <div v-if="selectedHelpDeskTopic.title" class="text-black">
                  Menu :
                  <span class="text-primary">{{ selectedHelpDeskTopic.title }}</span>
                </div>
              </div>
              <q-btn
                v-if="selectedHelpDeskTopic.title"
                color="primary"
                icon="o_add"
                label="Add"
                no-caps
                @click="onAddHelpDeskQuestions"
              />
            </div>
            <div
              v-if="selectedHelpDeskTopic?.title && (!helpDeskQuestions || helpDeskQuestions.length === 0)"
              class="text-center text-grey-6 q-pa-lg"
            >
              No Data Available
            </div>
            <div
              v-for="(helpDeskQuestion, helpDeskQuestionIndex) in helpDeskQuestions"
              :key="helpDeskQuestion.id"
              :class="['row items-center q-mb-md q-pa-sm', activeRowId && activeRowId === selectedHelpDeskTopic.id ? 'bg-grey-2' : '']"
              style="border: 1px solid #ccc;"
            >
              <div class="col">
                <template
                  v-if="helpDeskQuestion.showInputForNewHelpDeskQuestion ||
                    (activeEdit.rowId === helpDeskQuestion.id && activeEdit.field === 'helpDeskQuestion')"
                >
                  <div class="row items-center">
                    <q-input
                      v-model="helpDeskQuestion.question"
                      outlined
                      stack-label
                      hide-bottom-space
                      dense
                      class="col"
                      placeholder="Enter Menu"
                      hint="Press Enter to save"
                      @keyup.enter="saveOrUpdateHelpDeskQuestion(helpDeskQuestion, helpDeskQuestionIndex)"
                    >
                      <q-tooltip>
                        Press enter key to save Menu
                      </q-tooltip>
                    </q-input>
                    <q-btn
                      v-if="helpDeskQuestion.question && !helpDeskQuestion.showInputForNewHelpDeskQuestion"
                      icon="o_close"
                      size="xs"
                      color="black"
                      flat
                      round
                      dense
                      class="q-mx-sm q-mb-md"
                      @click="activeEdit = { rowId: null, field: null }"
                    />
                  </div>
                </template>

                <template v-else>
                  <div
                    class="row items-center justify-between q-pa-xs cursor-pointer"
                    @dblclick="activeEdit = { rowId: helpDeskQuestion.id, field: 'helpDeskQuestion' }"
                  >
                    <span class="q-ml-xs">{{ helpDeskQuestion.question }}</span>
                  </div>
                </template>
                <q-tooltip v-if="helpDeskQuestion.question && !helpDeskQuestion.showInputForNewHelpDeskTopic && activeEdit.rowId !== helpDeskQuestion.id">Double-click to edit</q-tooltip>
              </div>
              <q-icon
                v-if="helpDeskQuestion.question"
                name="o_notes"
                color="primary"
                size="sm"
                class="cursor-pointer q-mx-sm"
                @click="openDescriptionDialog(null, helpDeskQuestion)"
              >
                <q-tooltip>Description</q-tooltip>
              </q-icon>
              <q-icon
                v-else
                name="o_notes"
                color="grey-5"
                size="sm"
                class="q-mx-sm"
                disable
              >
                <q-tooltip>Enter menu first</q-tooltip>
              </q-icon>
              <q-icon
                v-if="hasValidDescription(helpDeskQuestion.description)"
                name="o_info"
                size="16px"
                class="q-mx-sm"
              >
                <q-tooltip class="text-wrap break-words RichTextEditor" max-width="300px">
                  <div v-html="helpDeskQuestion.description" />
                </q-tooltip>
              </q-icon>
              <q-toggle
                v-model="helpDeskQuestion.isActive"
                dense class="q-mx-md"
                @update:model-value="() => saveOrUpdateHelpDeskQuestion(helpDeskQuestion)"
              />
              <q-icon
                name="o_delete"
                color="negative"
                size="sm"
                class="cursor-pointer"
                @click="onDeleteHelpDeskQuestion(helpDeskQuestion, helpDeskQuestionIndex)"
              >
                <q-tooltip>Delete</q-tooltip>
              </q-icon>
              <q-dialog ref="dialogRef" v-model="isDescriptionDialogForQuestion[helpDeskQuestion.id]" persistent transition-show="scale" transition-hide="scale" @hide="onDialogHide">
                <q-card>
                  <q-card-section class="card-header with-tools position-relative">
                    <div class="text-black text-h2">
                      Description For
                      <span class="text-primary">{{ helpDeskQuestion.question }}</span>
                    </div>
                    <q-btn v-close-popup icon="o_close" class="close" flat round dense style="position: absolute; right: 5px; top: 10px;" />
                  </q-card-section>

                  <q-card-section>
                    <q-editor
                      v-model="helpDeskQuestion.description"
                      :model-value="helpDeskQuestion.description ?? ''"
                      :dense="$q.screen.lt.md"
                      :toolbar="toolbar"
                      :fonts="fonts"
                      placeholder="Type description here..."
                    />
                  </q-card-section>

                  <q-card-actions align="right">
                    <q-btn v-close-popup color="grey-8" flat dense bordered label="Close" @click="onDialogCancel" />
                    <q-btn color="primary" flat bordered label="Save" @click="saveOrUpdateHelpDeskQuestion(helpDeskQuestion, helpDeskQuestionIndex)" />
                  </q-card-actions>
                </q-card>
              </q-dialog>
            </div>
          </div>
        </div>
      </div>
    </q-card>
  </q-page>
</template>
<script setup>
import { ref, onMounted } from "vue";
import { notifyError, notifySuccess, zwConfirmDelete, zwConfirm } from "assets/utils";
import { useQuasar, useDialogPluginComponent } from "quasar";
import { useRouter } from "vue-router";

import helpDeskTopicsQuestionsService from "modules/helpdesk/helpDeskTopicsQuestions.service.js";

const $q = useQuasar();
const helpDeskTopics = ref([]);
const helpDeskQuestions = ref([]);
const activeRowId = ref(null);
const router = useRouter();
const isDescriptionDialogForQuestion = ref({});
const isDescriptionDialogForTopic = ref({});
const selectedHelpDeskTopic = ref({ name: "", id: null });
const activeEdit = ref({ rowId: null, field: null });
const helpDeskTopicId = ref(history.state?.id);
const isLoading = ref(false);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Get help desk topic by ID
// --------------------------------------------------------------------------------------------------------------------------------------------------
function getHelpDeskTopicById () {
  isLoading.value = true;
  helpDeskTopicsQuestionsService.getHelpDeskTopicById(helpDeskTopicId.value)
    .then((resp) => {
      helpDeskTopics.value = resp || [];
      if (!helpDeskTopics.value.length) {
        router.back();
      }
    })
    .catch((err) => {
      console.error(err);
      router.back();
    })
    .finally(() => {
      isLoading.value = false;
    });
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Get question list by help desk topic
// --------------------------------------------------------------------------------------------------------------------------------------------------
const getAllHelpDeskTopicQuestionList = (title, id) => {
  selectedHelpDeskTopic.value = { title, id };
  isLoading.value = true;

  helpDeskTopicsQuestionsService.getAllHelpDeskTopicQuestionList(id)
    .then((resp) => {
      helpDeskQuestions.value = resp;
      activeRowId.value = id;
    })
    .finally(() => {
      isLoading.value = false;
    });
};
// --------------------------------------------------------------------------------------------------------------------------------------------------
//  Add new help desk topic
// --------------------------------------------------------------------------------------------------------------------------------------------------
function onAddHelpDeskTopic () {
  helpDeskTopics.value.push({
    title: "",
    description: "",
    showInputForNewHelpDeskTopic: true,
    isActive: false,
    deleted: false
  });
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
//  Add new dropdown value
// --------------------------------------------------------------------------------------------------------------------------------------------------
function onAddHelpDeskQuestions () {
  if (!Array.isArray(helpDeskQuestions.value)) {
    helpDeskQuestions.value = [];
  }
  helpDeskQuestions.value.push({
    question: "",
    description: "",
    isActive: false,
    showInputForNewHelpDeskQuestion: true
  });
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Delete help desk topic
// --------------------------------------------------------------------------------------------------------------------------------------------------
const onDeleteHelpDeskTopic = async (helpDeskTopic, helpDeskTopicIndex) => {
  activeRowId.value = helpDeskTopic.id;
  try {
    if (!helpDeskTopic.id) {
      helpDeskTopics.value.splice(helpDeskTopicIndex, 1);
      return;
    }
    const resp = await helpDeskTopicsQuestionsService.checkTopicCanBeDeleted(helpDeskTopic.id);
    const canDelete = resp?.canDelete;
    if (canDelete) {
      zwConfirmDelete({ data: `${helpDeskTopic.title}` }, () => {
        helpDeskTopicsQuestionsService.deleteHelpDeskTopic(helpDeskTopic.id).then(() => {
          notifySuccess({ message: "Workspace deleted successfully." });
          helpDeskTopics.value.splice(helpDeskTopicIndex, 1);
          if (selectedHelpDeskTopic.value.id === helpDeskTopic.id) {
            selectedHelpDeskTopic.value = { title: "", id: null };
          }
          if (!helpDeskTopics.value.length) {
            router.back();
          }
        });
      });
    } else {
      zwConfirm({
        title: "Active Menus Found",
        message: "This workspace has active Menus. You cannot delete it.",
        data: `${helpDeskTopic.title}`,
        okLabel: "OK",
        cancel: false
      }, () => {
      });
    }
  } catch (error) {
    console.error("Error checking topic:", error);
  } finally {
    activeRowId.value = null;
  }
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Delete help desk question
// --------------------------------------------------------------------------------------------------------------------------------------------------
const onDeleteHelpDeskQuestion = (helpDeskQuestion, helpDeskQuestionIndex) => {
  if (!helpDeskQuestion.id) {
    helpDeskQuestions.value.splice(helpDeskQuestionIndex, 1);
    return;
  }
  zwConfirmDelete({ data: `${helpDeskQuestion.question}` }, () => {
    helpDeskTopicsQuestionsService.deleteHelpDeskQuestion(helpDeskQuestion.id).then(() => {
      notifySuccess({ message: "Menu is deleted successfully." });
      helpDeskQuestions.value.splice(helpDeskQuestionIndex, 1);
    });
  });
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Save or update help desk topic
// --------------------------------------------------------------------------------------------------------------------------------------------------
const saveOrUpdateHelpDeskTopic = (helpDeskTopic) => {
  if (!helpDeskTopic.title || !helpDeskTopic.title.trim()) {
    notifyError({ message: "Please enter a value for the Workspace." });
    return;
  }
  const isUpdate = !!helpDeskTopic.id; // true if updating

  const payload = {
    id: helpDeskTopic.id || null,
    title: helpDeskTopic.title,
    description: helpDeskTopic.description,
    isActive: helpDeskTopic.isActive
  };
  helpDeskTopicsQuestionsService.saveHelpDeskTopic(payload.id, payload).then((resp) => {
    if (!isUpdate) {
      helpDeskTopic.id = resp.id;
    }
    helpDeskTopic.showInputForNewHelpDeskTopic = false; // hide input
    activeEdit.value = { rowId: null }; // stop editing

    notifySuccess({
      message: isUpdate
        ? "Workspace updated successfully."
        : "Workspace saved successfully."
    });
    isDescriptionDialogForTopic.value = {
      ...isDescriptionDialogForTopic.value,
      [payload.id]: false
    };
  });
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Save or update help desk question
// --------------------------------------------------------------------------------------------------------------------------------------------------
const saveOrUpdateHelpDeskQuestion = (helpDeskQuestion) => {
  if (!helpDeskQuestion.question || !helpDeskQuestion.question.trim()) {
    notifyError({ message: "Please enter a value for the Menu." });
    return;
  }
  const isUpdate = !!helpDeskQuestion.id; // true if updating

  const payload = {
    id: helpDeskQuestion.id || null,
    topicId: selectedHelpDeskTopic.value.id,
    question: helpDeskQuestion.question,
    description: helpDeskQuestion.description,
    isActive: helpDeskQuestion.isActive
  };
  helpDeskTopicsQuestionsService.saveHelpDeskQuestion(payload.id, payload).then((resp) => {
    if (!isUpdate) {
      helpDeskQuestion.id = resp.id;
    }
    helpDeskQuestion.showInputForNewHelpDeskQuestion = false; // hide input
    activeEdit.value = { rowId: null }; // stop editing

    notifySuccess({
      message: isUpdate
        ? "Menu updated successfully."
        : "Menu saved successfully."
    });
    isDescriptionDialogForQuestion.value = {
      ...isDescriptionDialogForQuestion.value,
      [payload.id]: false
    };
  });
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Open description dialog
// --------------------------------------------------------------------------------------------------------------------------------------------------
function openDescriptionDialog (helpDeskTopic, helpDeskQuestion) {
  // Open dialog for topic only if topic exists
  if (helpDeskTopic) {
    isDescriptionDialogForTopic.value[helpDeskTopic.id] = true;
  }
  // Open dialog for question only if question exists
  if (helpDeskQuestion) {
    isDescriptionDialogForQuestion.value[helpDeskQuestion.id] = true;
  }
}

const hasValidDescription = (d = "") =>
  d.replace(/<[^>]*>/g, "").replace(/&nbsp;/gi, " ").trim() !== "";
// --------------------------------------------------------------------------------------------------------------------------------------------------
// Rich Editor Tools: Fonts and Toolbar
// --------------------------------------------------------------------------------------------------------------------------------------------------
const fonts = {
  arial: "Arial",
  arial_black: "Arial Black",
  comic_sans: "Comic Sans MS",
  courier_new: "Courier New",
  impact: "Impact",
  lucida_grande: "Lucida Grande",
  times_new_roman: "Times New Roman",
  verdana: "Verdana"
};

const toolbar = [
  [
    {
      label: $q.lang.editor.align,
      icon: $q.iconSet.editor.align,
      fixedLabel: true,
      list: "only-icons",
      options: ["left", "center", "right", "justify"]
    }
  ],
  ["bold", "italic", "strike", "underline"],
  ["token", "hr", "link", "custom_btn"],
  [
    {
      label: $q.lang.editor.formatting,
      icon: $q.iconSet.editor.formatting,
      list: "no-icons",
      options: ["p", "h1", "h2", "h3", "h4", "h5", "h6", "code"]
    },
    "removeFormat"
  ],
  ["quote", "unordered", "ordered", "outdent", "indent"],
  ["undo", "redo"],
  ["viewsource"]
];

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On page rendering
// --------------------------------------------------------------------------------------------------------------------------------------------------
onMounted(() => {
  getHelpDeskTopicById();
});
</script>
