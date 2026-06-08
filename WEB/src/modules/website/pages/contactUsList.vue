<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-md-2">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Contacts" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-md-5">
            <div class="row items-center q-ml-lg">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
          <div class="col-12 col-md-5">
            <div class="row items-center justify-end no-wrap">
              <div class="row items-center" style="flex-wrap: nowrap;">
                <div class="search-container" style="position: relative; display: flex; align-items: center; width: 320px;">
                  <q-input
                    v-model="searchText"
                    :loading="searchLoader"
                    outlined
                    dense
                    clearable
                    debounce="300"
                    placeholder="Search"
                    class="bg-white search-box"
                    style="flex: 1; border-top-right-radius: 0; border-bottom-right-radius: 0; border-top-right-radius: 0; max-width: 250px;"
                  >
                    <template #prepend>
                      <q-icon
                        name="o_search"
                      />
                    </template>
                  </q-input>
                  <q-btn
                    unelevated
                    :color="Object.keys(appliedFilters).length > 0 ? 'primary' : 'grey-7'"
                    text-color="white"
                    size=""
                    class="q-pa-xs q-mr-xs filter-btn"
                    style="height: 40px; border-top-left-radius: 0; border-bottom-left-radius: 0;"
                    @click.stop="showFilter = !showFilter"
                  >
                    <q-badge v-if="Object.keys(appliedFilters).length > 0" color="green" floating>
                      {{ Object.keys(appliedFilters).length }}
                    </q-badge>
                    <q-icon name="o_filter_alt" size="sm" color="white" class="q-mr-xs" />
                    <q-item-label class="text-xs fs-12">
                      <span class="block">Set/Clear</span>
                      <span class="block">FILTER</span>
                    </q-item-label>
                    <q-tooltip anchor="bottom middle" self="top middle">Advanced Filter</q-tooltip>
                  </q-btn>
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 450px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Full Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.fullName"
                            push
                            class="q-mx-sm w-100 h-auto"
                            hide-bottom-space
                            :dense="true"
                            type="text"
                          />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Email</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.email"
                            push
                            class="q-mx-sm w-100 h-auto"
                            hide-bottom-space
                            :dense="true"
                            type="text"
                          />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Subject</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.title"
                            push
                            class="q-mx-sm w-100 h-auto"
                            hide-bottom-space
                            :dense="true"
                            type="text"
                          />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm hidden">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Phone No</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.phoneNo"
                            push
                            class="q-mx-sm w-100 h-auto"
                            hide-bottom-space
                            :dense="true"
                            type="text"
                          />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Source</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.source"
                            push
                            class="q-mx-sm w-100 h-auto"
                            hide-bottom-space
                            :dense="true"
                            type="text"
                          />
                        </div>
                      </div>
                      <!-- Search and Clear Buttons -->
                      <div class="row justify-end q-gutter-sm q-mb-sm">
                        <q-btn style="width: 20%;" outline color="primary" label="Search" class="btnRounded" no-caps @click="() => { showFilter = false; onSearch(); }" />
                        <q-btn style="width: 20%;" outline color="grey-4" label="Clear" class="text-grey-9 btnRounded" no-caps @click="onClear" />
                        <q-btn style="width: 20%;" outline color="negative" label="Close" class="btnRounded" no-caps @click="() => { showFilter = false; }" />
                      </div>
                    </q-card>
                  </q-menu>
                </div>
              </div>
            </div>
          </div>
        </div>
      </q-card-section>

      <q-table
        ref="tableRef"
        v-model:pagination="pagination"
        :class="contactRows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
        :loading="loading"
        :rows="contactRows"
        :columns="contactColumns"
        row-key="id"
        separator="cell"
        no-data-label="No data available"
        binary-state-sort
        :rows-per-page-options="[20, 50, 100, 200, 500]"
        @request="getAllContactUsList"
      >
        <template #loading>
          <q-inner-loading showing color="primary">
            <q-spinner-ios size="40px" class="q-mt-xl" />
          </q-inner-loading>
        </template>
        <template #header="props">
          <q-tr :props="props" class="bg-primary text-white">
            <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
            <q-td style="width: 15%;">
              {{ props.row.fullName }}
            </q-td>
            <q-td style="width: 10%;">
              {{ props.row.email }}
            </q-td>
            <q-td style="width: 20%; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">
              <div class="RichTextEditor" v-html="props.row.title" />
            </q-td>
            <q-td style="width: 35%; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">
              <div class="RichTextEditor" v-html="props.row.message" />
            </q-td>
            <q-td style="width: 10%;">
              {{ props.row.source }}
            </q-td>
            <q-td style="width: 10%;">
              {{ props.row.contactedDate }}
            </q-td>
          </q-tr><q-separator />
        </template>
      </q-table>
    </q-card>
  </q-page>
</template>

<script setup>
// Import libraries
import { ref, onMounted, watch, computed } from "vue";
import { setLocalStorage, clearLocalStorage, getLocalStorage } from "assets/utils";
import contactUsService from "modules/website/website.service";

// Common variables
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);

// local storage values
const localStorageKey = "Contact Us";
const filterLocalStorage = getLocalStorage(localStorageKey);
const searchText = ref(filterLocalStorage?.searchText || "");
const fullName = filterLocalStorage ? filterLocalStorage.fullName : "";
const email = filterLocalStorage ? filterLocalStorage.email : "";
const phoneNo = filterLocalStorage ? filterLocalStorage.phoneNo : "";
const title = filterLocalStorage ? filterLocalStorage.title : "";
const source = filterLocalStorage ? filterLocalStorage.source : "";
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "contactedDate", descending: true, rowsPerPage: 20, page: 1 });

// Search variables
const search = ref({
  searchText,
  fullName,
  email,
  phoneNo,
  title,
  source
});

// Table variables
const tableRef = ref();
const contactRows = ref([]);
const activeRowId = ref(null);
const contactColumns = ref([
  { name: "fullName", label: "Full Name", field: "fullName", align: "left", sortable: true },
  { name: "email", label: "Email", field: "email", align: "left", sortable: true },
  { name: "title", label: "Subject", field: "title", align: "left", sortable: true },
  { name: "message", label: "Message", field: "message", align: "left", sortable: true },
  { name: "source", label: "Source", field: "source", align: "left", sortable: true },
  { name: "contactedDate", label: "Contacted Date", field: "contactedDate", align: "left", sortable: true }
]);

// Get/Map contact us list to table
const getAllContactUsList = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });
  contactUsService.getAllContactUs(payload).then((resp) => {
    contactRows.value = resp.contactUsLists;
    pagination.value.page = page;
    pagination.value.rowsPerPage = rowsPerPage;
    pagination.value.sortBy = sortBy;
    pagination.value.descending = descending;
    pagination.value.rowsNumber = resp.total;
  }).finally(() => {
    loading.value = false;
    searchLoader.value = false;
  });
};

// Search records as per parameters
const onSearch = () => {
  const propps = { pagination: pagination.value };
  getAllContactUsList(propps);
};

// Clear search
const onClear = () => {
  search.value.fullName = "";
  search.value.email = "";
  search.value.phoneNo = "";
  search.value.title = "";
  search.value.source = "";
  clearLocalStorage(localStorageKey);
  onSearch();
};

// ----------------------------
// Applied Filter Labels.
// ----------------------------
const appliedFilters = computed(() => ({
  ...(search.value.fullName ? { "Full Name": search.value.fullName } : {}),
  ...(search.value.email ? { Email: search.value.email } : {}),
  ...(search.value.phoneNo ? { "Phone No": search.value.phoneNo } : {}),
  ...(search.value.title ? { Subject: search.value.title } : {}),
  ...(search.value.source ? { Source: search.value.source } : {})
}));

function onClearFilters (key) {
  if (key === "Full Name") {
    search.value.fullName = "";
  } else if (key === "Email") {
    search.value.email = "";
  } else if (key === "Phone No") {
    search.value.phoneNo = "";
  } else if (key === "Subject") {
    search.value.title = "";
  } else if (key === "Source") {
    search.value.source = "";
  }
  delete appliedFilters.value[key];
  getAllContactUsList({ pagination: pagination.value });
}

// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => searchText.value, () => {
  if (searchText.value) searchLoader.value = true;
  getAllContactUsList({ pagination: pagination.value });
});

// On page rendering
onMounted(() => {
  tableRef.value.requestServerInteraction();
});

</script>
