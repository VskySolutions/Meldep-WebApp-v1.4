<!-- eslint-disable vue/no-v-html -->
<template>
  <q-page padding>
    <q-inner-loading :showing="loading" color="primary" class="z-max" />
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-xs-2 col-sm-1 col-md-2 col-lg-3 col-xl-2">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Tickets" icon="o_local_activity" />
              <q-breadcrumbs-el label="Manage" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-xs-3 col-sm-2 col-md-2 col-lg-2 col-xl-3">
            <div class="row items-center">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge>
                {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
          <div class="col-12 col-xs-7 col-sm-9 col-md-8 col-lg-7 col-xl-7">
            <div class="row items-center justify-end no-wrap">
              <div class="row items-center q-mr-xs">
                <div class="search-container position-relative">
                  <searchFilterBar
                    v-model="search.searchText"
                    :loading="searchLoader"
                    :applied-filters="appliedFilters"
                    class="search-bar"
                    @toggle-filter="showFilter = !showFilter"
                  />
                  <!-- Dropdown Content -->
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 450px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Ticket No</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.ticketNo"
                            clearable
                            class="q-mx-sm w-100 h-auto"
                            mask="#####"
                            fill-input
                            :dense="true"
                          >
                            <template #prepend>
                              <span class="fs-13">{{ prefixRef }} -</span>
                            </template>
                          </q-input>
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Title</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.title"
                            clearable
                            class="q-mx-sm w-100 h-auto"
                            fill-input
                            :dense="true"
                          />
                        </div>
                      </div>
                      <multiSelectDropdown
                        v-if="role === 'admin' || roleSupportTeam"
                        v-model="search.employeeEmails"
                        label="Requester"
                        :options="requesterNameForDropdown.list.value"
                        :filter="requesterNameForDropdown.filter"
                      />
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Assigned To</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.assignedToId"
                            class="q-mx-sm w-100 h-auto"
                            stack-label
                            hide-bottom-space
                            clearable
                            use-input
                            :dense="true"
                            :options="assignedToList"
                            emit-value
                            map-options
                            :popup-content-class="customPopupContentClass"
                          />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm hidden">
                        <div class="col-lg-4 col-md-4 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Company Name</label>
                        </div>
                        <div class="col-lg-8 col-md-8 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.companyIds"
                            push
                            class="q-mx-sm w-100 h-auto"
                            clearable
                            use-input
                            use-chips
                            transition-show="jump-up"
                            transition-hide="jump-up"
                            hide-bottom-space
                            :dense="true"
                            multiple
                            fill-input
                            input-debounce="0"
                            :options="helpDeskCompanyList"
                            option-value="value"
                            option-label="text"
                            emit-value
                            map-options
                            :popup-content-class="customPopupContentClass"
                            @filter="getAllHelpDeskCompanyListForFilter"
                          >
                            <template #option="{ itemProps, opt, selected, toggleOption }">
                              <q-item v-bind="itemProps">
                                <q-item-section>
                                  <div class="row q-col-gutter-x-md items-center selection-drodown-item">
                                    <q-checkbox :model-value="selected" @update:model-value="toggleOption(opt)" />
                                    <!-- <span>{{ opt.text.replace(/TM\b/g, '') }}</span> -->
                                    <span>{{ (opt?.text || '').replace(/TM\b/g, '') }}</span>
                                  </div>
                                </q-item-section>
                              </q-item>
                            </template>
                          </q-select>
                        </div>
                      </div>
                      <multiSelectDropdown
                        v-model="search.statusIds"
                        label="Status"
                        :options="helpDeskStatusDropdown.list.value"
                        :filter="helpDeskStatusDropdown.filter"
                        :isShowAll="true"
                      />
                      <multiSelectDropdown
                        v-model="search.priorityIds"
                        label="Priority"
                        :options="helpDeskPriorityDropdown.list.value"
                        :filter="helpDeskPriorityDropdown.filter"
                        :isShowAll="true"
                      />
                      <multiSelectDropdown
                        v-model="search.topicIds"
                        label="Workspaces"
                        :options="helpDeskActiveWorkspaceDropdown.list.value"
                        :filter="helpDeskActiveWorkspaceDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.questionIds"
                        label="Menus"
                        :disable="!search.topicIds > 0"
                        :options="helpDeskMenusDropdown.list.value"
                        :filter="helpDeskMenusDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.categoryIds"
                        label="Category"
                        :options="helpDeskCategoryDropdown.list.value"
                        :filter="helpDeskCategoryDropdown.filter"
                        :isShowAll="true"
                      />
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">From Date</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div class="input-group q-mx-sm w-100 h-auto">
                            <q-input v-model="search.ticketFromDate" fill-input dense mask="##/##/####">
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                    <q-date v-model="search.ticketFromDate" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
                                  </q-popup-proxy>
                                </q-icon>
                              </template>
                            </q-input>
                          </div>
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">To Date</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div class="input-group q-mx-sm w-100 h-auto">
                            <q-input v-model="search.ticketToDate" fill-input dense mask="##/##/####">
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                    <q-date v-model="search.ticketToDate" mask="MM/DD/YYYY" :options="disableBeforeStartDate" @update:model-value="() => $refs.qDateProxy.hide()" />
                                  </q-popup-proxy>
                                </q-icon>
                              </template>
                            </q-input>
                          </div>
                        </div>
                      </div>
                      <!-- Search and Clear Buttons -->
                      <div class="row justify-end q-gutter-sm q-mb-sm">
                        <q-btn style="width: 20%;" outline color="primary" label="Search" class="btnRounded" no-caps @click="() => { showFilter = false; onAdvanceSearch(); }" />
                        <q-btn style="width: 20%;" outline color="grey-4" label="Clear" class="text-grey-9 btnRounded" no-caps @click="onAdvanceClear" />
                        <q-btn style="width: 20%;" outline color="negative" label="Close" class="btnRounded" no-caps @click="() => { showFilter = false; }" />
                      </div>
                    </q-card>
                  </q-menu>
                </div>
              </div>
              <q-menu v-model="showHelpDeskTopicsOptions" anchor="bottom right" self="top right" no-parent-event style="width: 320px;">
                <q-card class="q-pa-sm">
                  <div class="text-subtitle2 q-mt-sm q-mb-sm">Manage Ticket Workspace & Menu</div>
                  <q-separator />
                  <q-list style="min-width: 200px">
                    <q-item
                      v-for="opt in helpDeskTopics"
                      :key="opt.id"
                      clickable
                      :active="selectedField === opt.id"
                      active-class="bg-primary text-white"
                      @click="$router.push({ path: '/help-desk/topics-questions/list', state: { id: opt.id } })"
                    >
                      <q-item-section>{{ opt.title }}</q-item-section>
                    </q-item>
                  </q-list>
                </q-card>
              </q-menu>
              <div class="q-gutter-sm ActionButtons">
                <q-btn
                  icon="o_view_list"
                  class="text-white btnRounded bg-primary q-px-sm"
                  :disable="$route.path === '/help-desk'"
                  @click="$router.push({ path: '/help-desk' })"
                >
                  <q-tooltip>List View</q-tooltip>
                </q-btn>
                <q-btn
                  v-if="user?.roles?.includes('admin')"
                  icon="o_playlist_add"
                  outline
                  no-caps
                  class="text-primary btnRounded q-ml-sm"
                  @click="showHelpDeskTopicsOptions = true"
                >
                  <q-tooltip>Manage Ticket Workspace & Menu</q-tooltip>
                </q-btn>
                <q-btn
                  icon="o_add"
                  outline
                  label="Add"
                  no-caps
                  class="text-primary btnRounded"
                  @click="onHelpDeskAdd(refreshHelpDeskList)"
                >
                  <q-tooltip>Add Ticket</q-tooltip>
                </q-btn>
                <q-btn
                  icon="o_chevron_left"
                  outline
                  label="Back"
                  no-caps
                  class="text-primary q-ml-sm"
                  @click="$router.back()"
                >
                  <q-tooltip anchor="bottom middle" self="top middle">Back To List</q-tooltip>
                </q-btn>
              </div>
            </div>
          </div>
        </div>
        <div class="row q-mt-md">
          <div class="col-xxl-2 col-xl-2 col-lg-3 col-md-3 col-sm-12 col-xs-12 q-mb-md">
            <q-table
              ref="tableRef"
              v-model:pagination="pagination"
              :class="(rows.length > 0 ? 'my-sticky-header-table' : '') + 'Custom-DataTable TicketTable'"
              :loading="loading"
              :rows="rows"
              :columns="helpDeskColumns"
              row-key="id"
              separator="cell"
              no-data-label="No data available"
              binary-state-sort
              :rows-per-page-options="[20, 50, 100, 200, 500]"
              :filter="searchText"
              style="height: 100vh;"
              @request="getAllHelpDesks"
            >
              <template #loading>
                <q-inner-loading showing color="primary">
                  <q-spinner-ios size="40px" class="q-mt-xl" />
                </q-inner-loading>
              </template>
              <template #header="props">
                <q-tr :props="props" class="bg-primary text-white">
                  <q-th v-for="col in props.cols" :key="col.name" :props="props">
                    {{ col.label }}
                  </q-th>
                  <q-th auto-width class="text-center hidden">Actions</q-th>
                </q-tr>
              </template>
              <template #body="props">
                <q-tr :props="props" :class="['cursor-pointer', helpDeskId == props.row.id ? 'bg-green-2' : '']" @click="onHelpDeskSelect(props.row)">
                  <q-td class="highlight-text" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">
                    <div class="column q-gutter-xs" style="white-space: normal; word-break: break-word;">
                      <div class="row items-center justify-between">
                        <div class="text-caption text-weight-bold text-black">
                          #{{ props.row.displayTicketNo }}
                        </div>
                        <div>
                          <q-badge :color="getStatusColor(props.row.statusText)" outline class="text-caption text-bold q-mr-xs">
                            {{ props.row.statusText }}
                            <q-tooltip>Status</q-tooltip>
                          </q-badge>
                          <q-badge :color="getPriorityColor(props.row.priority.dropDownValue)" outline class="text-caption text-bold">
                            {{ props.row.priority.dropDownValue }}
                            <q-icon name="o_priority_high" />
                            <q-tooltip>Priority</q-tooltip>
                          </q-badge>
                        </div>
                      </div>
                      <div class="text-black fs-15">
                        {{ props.row.title }}
                      </div>
                      <!-- <div class="text-caption text-grey-8">
                        Request By - <span class="text-primary">{{ props.row.employee?.person?.fullName ?? props.row.requesterEmail }}</span>
                      </div> -->
                      <div class="row items-center no-wrap text-caption text-grey-8">
                        <div>
                          Requester:
                          <!-- <span class="text-primary"> -->
                          <div class="row items-center no-wrap text-primary">
                            <!-- {{ props.row.employee?.person?.fullName ?? props.row.requesterEmail }} -->
                            <div v-if="props.row.requesterEmail">
                              {{ props.row.requesterEmail }}
                            </div>
                            <div v-else-if="props.row.requesterId">
                              {{ props.row.employee?.person?.primaryEmailAddress }}
                            </div>
                            <q-icon
                              v-if="props.row.employee.person.id"
                              name="o_person"
                              class="cursor-pointer q-pl-sm"
                              size="xs"
                              @click.stop="onPersonView(props.row.employee.person.id)"
                            >
                              <q-tooltip>View Requester</q-tooltip>
                            </q-icon>
                          </div>
                        </div>
                        <q-space />
                        <div class="relative-position q-mr-sm">
                          <q-icon name="o_email" size="xs" class="cursor-pointer">
                            <q-tooltip>Email Replies</q-tooltip>
                          </q-icon>
                          <q-badge
                            v-if="props.row.emailRepliesCount > 0"
                            color="green"
                            text-color="white"
                            floating
                            rounded
                            class="text-caption"
                            style="position: absolute; right: -10px; top: -11px;"
                          >
                            {{ props.row.emailRepliesCount }}
                          </q-badge>
                        </div>
                        <div class="relative-position q-mr-sm">
                          <q-icon name="o_assignment" size="xs" class="cursor-pointer" @click.stop="onAddNote(props.row.id, 'Help Desk Notes', props.row.id, props.row.title, props.row.title, !['Closed','Cancelled'].includes(props.row.statusText))">
                            <q-tooltip>Notes</q-tooltip>
                          </q-icon>
                          <q-badge
                            v-if="props.row.helpDeskNotesCount > 0"
                            color="green"
                            text-color="white"
                            floating
                            rounded
                            class="text-caption"
                            style="position: absolute; right: -8px; top: -11px;"
                          >
                            {{ props.row.helpDeskNotesCount }}
                          </q-badge>
                        </div>
                      </div>
                    </div>
                  </q-td>
                </q-tr>
                <q-separator />
              </template>
            </q-table>
          </div>
          <div class="col-xxl-10 col-xl-10 col-lg-9 col-md-9 col-sm-12 col-xs-12 q-mb-md">
            <div class="row">
              <div class="col-xxl-9 col-xl-8 col-lg-8 col-md-7 col-sm-6 col-xs-12 TicketReplies email-section q-pl-sm" style="height: 100vh;">
                <div class="box-shadow full-height column">
                  <!-- <div class="text-white bg-primary q-mb-sm q-pa-sm"><q-icon name="o_inbox" size="sm" /> Email Conversations</div>
                  <div v-if="helpDeskId && loggedInEmail && employeeId">
                    <emailReplies :key="helpDeskId" :help-desk-id="helpDeskId" :logged-in-email="loggedInEmail" :employee-id="employeeId" :twilio-email-id="twilioEmailId" />
                  </div>
                  <div v-else class="q-pa-sm text-center text-red">
                    No data available
                  </div> -->
                  <div class="bg-primary text-white q-mb-sm q-pa-xs">
                    <div class="row items-center justify-between">
                      <div class="row items-center">
                        <q-icon name="o_email" size="sm" class="q-mr-sm" />
                        <span>Email Conversations</span>
                      </div>

                      <div class="row items-center q-gutter-sm">
                        <q-btn
                          dense
                          unelevated
                          icon="o_refresh"
                          color="white"
                          text-color="primary"
                          :disable="!helpDeskId"
                          @click="refreshEmails"
                        >
                          <q-tooltip>Refresh conversation</q-tooltip>
                        </q-btn>
                        <q-btn
                          v-if="!['Closed','Cancelled'].includes(model.statusText)"
                          dense
                          unelevated
                          color="white"
                          text-color="primary"
                          :icon="showReplyEditor ? 'o_close' : 'o_reply'"
                          :label="showReplyEditor ? 'Close' : 'Reply'"
                          :disable="!helpDeskId"
                          @click="onReplyToggle"
                        />
                        <!-- Show system emails checkbox -->
                        <q-checkbox
                          v-model="showSystemEmails"
                          label="Show system emails?"
                          dense
                          color="primary"
                          :disable="!helpDeskId"
                          class="white-border-checkbox text-white"
                        />
                      </div>
                    </div>
                  </div>
                  <q-scroll-area class="col">
                    <!-- EMAIL COMPONENT -->
                    <emailReplies
                      v-if="helpDeskId"
                      :key="emailKey"
                      v-model="replyText"
                      :help-desk-id="helpDeskId"
                      :logged-in-email="loggedInEmail"
                      :employee-id="employeeId"
                      :twilio-email-id="twilioEmailId"
                      :show-reply="showReplyEditor"
                      :title="emailSubject"
                      :show-system-emails="showSystemEmails"
                    />
                    <div v-else class="q-pa-sm text-center text-red">
                      No data available
                    </div>
                  </q-scroll-area>
                </div>
              </div>
              <div class="col-xxl-3 col-xl-4 col-lg-4 col-md-5 col-sm-6 col-xs-12 TicketAttributes ticket-details q-pl-sm">
                <div class="box-shadow">
                  <div class="text-white bg-primary q-mb-sm q-pa-sm"><q-icon name="o_edit_attributes" size="sm" /> Ticket Attributes</div>
                  <div v-if="model.displayTicketNo" class="q-pb-md text-grey-10">
                    <div class="q-px-sm q-pb-sm">
                      <span class="text-primary q-mr-xs">Ticket No.:</span>
                      <span>#{{ model.displayTicketNo }}</span>
                    </div>
                    <div class="q-px-sm q-pb-sm">
                      <div class="row items-center no-wrap">
                        <span class="text-primary q-mr-xs">
                          Requester:
                        </span>
                        <span v-if="model.requesterEmail" class="text-black ellipsis">
                          {{ model.requesterEmail }}
                        </span>
                        <span v-else-if="model.requesterId" class="text-black ellipsis">
                          {{ model.employee?.person?.primaryEmailAddress }}
                        </span>
                        <q-icon
                          v-if="model.employee.person.id"
                          name="o_person"
                          class="cursor-pointer q-ml-sm"
                          size="xs"
                          @click="onPersonView(model.employee.person.id)"
                        >
                          <q-tooltip>View Requester</q-tooltip>
                        </q-icon>
                      </div>
                    </div>
                    <div class="q-px-sm q-pb-sm">
                      <span class="text-primary q-mr-xs">Title:</span>
                      <span>{{ model.title }}</span>
                    </div>
                    <div class="q-px-sm q-pb-sm">
                      <span class="text-primary q-mr-xs">Workspace:</span>
                      <span>{{ model.helpDeskTopic.title }}</span>
                    </div>
                    <div class="q-px-sm q-pb-sm">
                      <span class="text-primary q-mr-xs">Menu:</span>
                      <span>{{ model.helpDeskTopicQuestions.question }}</span>
                    </div>
                    <div class="q-px-sm q-pb-sm">
                      <span class="text-primary q-mr-xs">Category:</span>
                      <span>{{ model.category.dropDownValue }}</span>
                    </div>
                    <div class="q-px-sm q-pb-sm hidden">
                      <span class="text-primary q-mr-xs">Company:</span>
                      <span>{{ model.company?.name }}</span>
                    </div>
                    <div class="q-px-sm q-pb-sm hidden">
                      <div class="text-primary q-mb-xs">Company:</div>
                       <span v-if="!isRoleAdminOrSupport" class="text-black">{{ model.company?.name }}</span>
                      <span
                        v-if="isStatusDisabled(model.statusId) === 'Completed' || isStatusDisabled(model.statusId) === 'Closed' || isStatusDisabled(model.statusId) === 'Cancelled'"
                        class="text-black"
                      >
                        {{ model.company?.name }}
                      </span>
                      <div v-else class="row items-center no-wrap text-black">
                        <q-select
                          v-model="model.companyId"
                          outlined
                          use-input
                          hide-bottom-space
                          dense
                          :options="customerList"
                          class="help-desk-status-list"
                          option-value="value"
                          option-label="text"
                          emit-value
                          map-options
                          style="width: 93%;"
                          :loading="formLoading.company"
                          @filter="getAllCustomerListFilter"
                          @update:model-value="onChangeCompany(model.id, model.companyId)"
                        />
                      </div>
                    </div>
                    <div class="q-px-sm q-pb-sm">
                      <div class="text-primary q-mb-xs">Status:</div>
                      <b v-if="!isAssignedToCurrentUser && !isRoleAdminOrSupport" :class="'text-' + getStatusColor(model.statusText)">{{ model.statusText }}</b>
                      <span
                        v-else-if="isStatusDisabled(model.statusId) === 'Closed' || isStatusDisabled(model.statusId) === 'Cancelled'"
                        :class="'text-' + getStatusColor(model.statusText)"
                        class="row items-center no-wrap"
                      >
                        {{ model.statusText }}
                        <div v-if="model.statusText == 'Closed' && model.previousStatusText == 'Open'" class="q-ml-sm">
                          <q-icon
                            name="o_comment"
                            size="xs"
                            class="cursor-pointer"
                            color="black"
                          >
                            <q-tooltip>Add Comments</q-tooltip>
                            <q-popup-edit
                              v-model="model.closingComment"
                              anchor="top middle"
                              self="bottom middle"
                              buttons
                              persistent
                              label-set="Save"
                              label-cancel="Cancel"
                              @save="val => onSaveComment(model.id, val)"
                            >
                              <template #default="scope">
                                <div class="relative-position q-pa-sm" style="min-width: 260px;">
                                  <q-btn
                                    icon="o_close"
                                    flat
                                    round
                                    dense
                                    size="sm"
                                    class="absolute-top-right"
                                    @click="scope.cancel"
                                  />

                                  <div class="text-subtitle2 q-mb-xs">Comment<span class="text-grey-6 fs-12"> (Please leave a comment when closing a ticket directly from New or Open status.)</span></div>

                                  <q-input
                                    v-model="scope.value"
                                    type="textarea"
                                    outlined
                                    autogrow
                                    dense
                                  />
                                </div>
                              </template>
                            </q-popup-edit>
                          </q-icon>
                        </div>
                      </span>
                      <div v-else
                        :class="{ 'hoverable-cell' : model.id }"
                        class="row items-center no-wrap"
                        @click="activeEdit = { rowId: model.id, field: 'status' }"
                      >
                        <quickEditSingleSelect
                          field="status"
                          :row-id="model.id"
                          :value="model.statusId"
                          :display-value="model.statusText"
                          :editable="model.id"
                          :disable="model.statusText === 'Closed' || model.statusText === 'Cancelled'"
                          :options="getVisibleStatusOptions(model)"
                          :active-edit="activeEdit"
                          :show-history="false"
                          :loading="formLoading.status"
                          @cancel="activeEdit = { rowId: null, field: null }"
                          @submit="({ rowId, value }) => onSubmitHelpDeskStatusFormLoader(rowId, value, refreshHelpDeskList, refreshGetHelpDesk)"
                          :widthpx="'200px'"
                        />
                        <div v-if="model.statusText == 'Closed' && model.previousStatusText == 'Open'" class="q-ml-sm">
                          <q-icon
                            name="o_comment"
                            size="xs"
                            class="cursor-pointer"
                          >
                            <q-tooltip>Add Comments</q-tooltip>
                            <q-popup-edit
                              v-model="model.closingComment"
                              anchor="top middle"
                              self="bottom middle"
                              buttons
                              persistent
                              label-set="Save"
                              label-cancel="Cancel"
                              @save="val => onSaveComment(model.id, val)"
                            >
                              <template #default="scope">
                                <div class="relative-position q-pa-sm" style="min-width: 260px;">
                                  <q-btn
                                    icon="o_close"
                                    flat
                                    round
                                    dense
                                    size="sm"
                                    class="absolute-top-right"
                                    @click="scope.cancel"
                                  />

                                  <div class="text-subtitle2 q-mb-xs">Comment<span class="text-grey-6 fs-12"> (Please leave a comment when closing a ticket directly from New or Open status.)</span></div>

                                  <q-input
                                    v-model="scope.value"
                                    type="textarea"
                                    outlined
                                    autogrow
                                    dense
                                  />
                                </div>
                              </template>
                            </q-popup-edit>
                          </q-icon>
                        </div>
                      </div>
                    </div>
                    <div class="q-px-sm q-pb-sm">
                      <div class="text-primary q-mb-xs">Priority:</div>
                      <b
                        v-if="!isAssignedToCurrentUser && !isRoleAdminOrSupport"
                        :class="'text-' + getPriorityColor(model.priority.dropDownValue)"
                      >
                        {{ model.priority.dropDownValue }}
                      </b>
                      <span
                        v-else-if="isStatusDisabled(model.statusId) === 'Completed' || isStatusDisabled(model.statusId) === 'Closed' || isStatusDisabled(model.statusId) === 'Cancelled'"
                        :class="'text-' + getPriorityColor(model.priority.dropDownValue)"
                      >
                        {{ model.priority.dropDownValue }}
                      </span>
                      <div v-else
                        :class="{ 'hoverable-cell' : model.id }"
                        class="row items-center no-wrap"
                        @click="activeEdit = { rowId: model.id, field: 'priority' }"
                      >
                        <quickEditSingleSelect
                          field="priority"
                          :row-id="model.id"
                          :value="model.priorityId"
                          :display-value="model.priority.dropDownValue"
                          :editable="model.id"
                          :disable="isStatusDisabled(model.statusId) === 'Completed' || isStatusDisabled(model.statusId) === 'Closed' || isStatusDisabled(model.statusId) === 'Cancelled'"
                          :options="helpDeskPriorityDropdown.list.value"
                          :active-edit="activeEdit"
                          :show-history="false"
                          :loading="formLoading.priority"
                          @cancel="activeEdit = { rowId: null, field: null }"
                          @submit="({ rowId, value }) => onSubmitHelpDeskPriorityFormLoader(rowId, value, refreshHelpDeskList, refreshGetHelpDesk)"
                          :widthpx="'200px'"
                        />
                      </div>
                    </div>
                    <div class="q-px-sm q-pb-sm">
                      <div class="text-primary q-mb-xs">Assigned To:</div>
                      <span v-if="role !== 'admin'">{{ model.assignedTo?.person?.fullName }}</span>
                      <span
                        v-else-if="isStatusDisabled(model.statusId) === 'Completed' || isStatusDisabled(model.statusId) === 'Closed' || isStatusDisabled(model.statusId) === 'Cancelled'"
                      >
                        {{ model.assignedTo?.person?.fullName }}
                        <q-icon
                          v-if="model.assignedToCount > 1"
                          name="o_history"
                          class="cursor-pointer"
                          size="xs"
                          @click.stop="getStatusChangeLog(model.id, model.title, 'Assigned To')"
                        >
                          <q-tooltip>Data Change Log</q-tooltip>
                        </q-icon>
                      </span>
                      <div
                        v-else
                        class="flex items-center common-q-td"
                        :class="{ 'hoverable-cell' : model.id }"
                        @click="activeEdit = { rowId: model.id, field: 'assignedTo' }"
                      >
                        <quickEditSingleSelect
                          field="assignedTo"
                          :row-id="model.id"
                          :value="model.assignedToId"
                          :display-value="model.assignedTo.person.fullName"
                          :editable="model.id"
                          :options="supportTeamUserForDropdown.list.value"
                          :active-edit="activeEdit"
                          :show-history="false"
                          :disable="model.statusText === 'Completed' || model.statusText === 'Closed' || model.statusText === 'Cancelled'"
                          :loading="formLoading.assignedTo"
                          @cancel="activeEdit = { rowId: null, field: null }"
                          @submit="({ rowId, value }) => onSubmitAssignedToFormLoader(rowId, value, refreshHelpDeskList, refreshAllUserListByRoleForDropdown, refreshGetHelpDesk)"
                          :widthpx="'150px'"
                        />
                        <q-icon
                          v-if="model.assignedToCount > 1"
                          name="o_history"
                          class="cursor-pointer"
                          size="xs"
                          @click.stop="getStatusChangeLog(model.id, model.title, 'Assigned To')"
                        >
                          <q-tooltip>Data Change Log</q-tooltip>
                        </q-icon>
                      </div>
                    </div>
                    <div class="q-px-sm q-pb-sm">
                      <span class="text-primary q-mr-xs">Description:
                        <q-icon
                          name="o_fullscreen"
                          class="cursor-pointer text-black"
                          size="xs"
                          @click.stop="onHelpDeskView(model.id, model.title, model.employeeId, model.employee.person.primaryEmailAddress, model.twilioEmailId, 'Help Desk Notes', refreshHelpDeskList, refreshAllUserListByRoleForDropdown)"
                        >
                          <q-tooltip>View Description</q-tooltip>
                        </q-icon>
                      </span>
                    </div>
                    <div class="q-px-sm q-pb-sm">
                      <span class="text-primary q-mr-xs">Created By:</span>
                      <span>{{ model.createdBy?.person?.fullName }}</span>
                    </div>
                    <div class="q-px-sm q-pb-sm">
                      <span class="text-primary q-mr-xs">Created Date:</span>
                      <span>{{ model.createdOnUtc }}</span>
                    </div>
                    <div class="q-px-sm q-pb-sm">
                      <span class="text-primary q-mr-xs">Updated By:</span>
                      <span>{{ model.updatedBy?.person?.fullName }}</span>
                    </div>
                    <div class="q-px-sm q-pb-sm">
                      <span class="text-primary q-mr-xs">Update Date:</span>
                      <span>{{ model.updatedOnUtc }}</span>
                    </div>
                  </div>
                  <div v-else class="q-pa-sm text-center text-red">
                    No data available
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
    </q-card>
  </q-page>
</template>

<script setup>
import _ from "lodash";
import { useQuasar } from "quasar";
import { useAuthStore } from "stores/auth";
import useFilters from "composables/useFilters";
import { ref, onMounted, watch, computed, nextTick } from "vue";
import { notifySuccess, notifyError } from "assets/utils";

import emailReplies from "modules/helpdesk/components/_emailReplies.vue";
import siteStatusLog from "modules/sites/components/_siteModifiedLogs.vue";
import customerService from "src/modules/customer/customer.service";
import helpDeskService from "modules/helpdesk/helpDesk.service";
import helpDeskTopicsQuestionsService from "modules/helpdesk/helpDeskTopicsQuestions.service.js";
import addNote from "modules/common/components/addNote.vue";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import quickEditSingleSelect from "src/components/dataTable/_quickEditSingleSelect.vue";

// Shared DataTable Features
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";
import helpDeskModule from "src/modules/helpdesk/utils/dropdowns.js";

// Shared Helpdesk Dialogs
import {
  initHelpDeskDialogs,
  onHelpDeskView,
  onHelpDeskAdd
} from "src/modules/helpdesk/utils/dialogs.js";

// Shared Person Dialogs
import {
  initPersonDialogs,
  onPersonView
} from "src/modules/person/utils/dialogs.js";

// Shared HelpDesk Actions
import {
  initHelpDeskActions,
  onSubmitHelpDeskStatusFormLoader,
  onSubmitHelpDeskPriorityFormLoader,
  onSubmitAssignedToFormLoader,
  formLoading
} from "src/modules/helpdesk/utils/actions.js";

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------------

const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const showHelpDeskTopicsOptions = ref(false);
const emailKey = ref(0);
const showReplyEditor = ref(false);
const showSystemEmails = ref(false);
const { toDate } = useFilters();
const $q = useQuasar();
const authStore = useAuthStore();
const activeEdit = ref({ rowId: null, field: null });

const assignedToList = ref(["Me", "View All"]);

const user = authStore.user;
const currentUserId = user.id;
const isRoleAdminOrSupport = user?.roles?.includes("support team") || user?.roles?.includes("admin");
const isAssignedToCurrentUser = ref(false);
const role = user?.roles?.includes("admin") ? "admin" : "";
const roleSupportTeam = !!user?.roles?.includes("support team");
const loginUserEmployeeId = user?.employeeId;
const siteId = computed(() => authStore.user?.siteId);

const loggedInEmail = ref("");
const emailSubject = ref("");
const employeeId = ref("");
const twilioEmailId = ref("");
const helpDeskTopics = ref([]);
const replyText = ref("");
const prefixRef = ref("");
const isSearchTriggered = ref(false);

// ----------------------------------------------------------------------------------------------------------------
// local storage values
// ----------------------------------------------------------------------------------------------------------------

const {
  search,
  pagination,
  activeRowId,
  getTableState,
  saveDataTableState
} = useSiteTableState({
  storageKey: "helpdesks-CardView",
  siteId,
  defaultSearch: {
    searchText: "",
    title: "",
    ticketFromDate: "",
    ticketToDate: "",
    ticketNo: 0,
    employeeEmails: [],
    companyIds: [],
    categoryIds: [],
    statusIds: [],
    priorityIds: [],
    topicIds: [],
    questionIds: [],
    assignedToId: null
  },
  defaultPagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  }
});

const tableState = getTableState();

const helpDeskId = ref(tableState?.helpDeskId || null);
const selectedHelpDeskId = helpDeskId;
// ----------------------------------------------------------------------------------------------------------------
// Define model values
// ----------------------------------------------------------------------------------------------------------------

const model = ref({
  title: "",
  description: "",
  assignedToCount: "",
  employeeId: "",
  assignedToId: "",
  priorityId: "",
  twilioEmailId: "",
  employee: {
    person: {
      fullName: "",
      primaryEmailAddress: ""
    }
  },
  helpDeskTopic: {
    title: ""
  },
  helpDeskTopicQuestions: {
    question: ""
  },
  priority: {
    dropDownValue: ""
  },
  assignedTo: {
    person: {
      fullName: "",
      primaryEmailAddress: ""
    }
  }
});

const refreshEmails = () => {
  emailKey.value++;
};

// ----------------------------------------------------------------------------------------------------------------
// Table variables
// ----------------------------------------------------------------------------------------------------------------

const rows = ref([]);
const helpDeskColumns = [{ name: "ticketNo", label: "Tickets", align: "left", field: "name", sortable: true }];

const getAllHelpDesks = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  search.value.ticketFromDate = search.value.ticketFromDate === "" ? null : toDate(search.value.ticketFromDate);
  search.value.ticketToDate = search.value.ticketToDate === "" ? null : toDate(search.value.ticketToDate);

  if (search.value.topicIds > 0 && search.value.topicIds > 0) { helpDeskMenusDropdown.load(search.value.topicIds); }
  if (search.value.ticketFromDate === "") { search.value.ticketFromDate = null; }
  if (search.value.ticketToDate === "") { search.value.ticketToDate = null; }

  search.value.ticketNo = (search.value.ticketNo === "" || search.value.ticketNo === null) ? 0 : Number(search.value.ticketNo);
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };

  saveDataTableState({
    search: search.value,
    pagination: props.pagination,
    activeRowId: activeRowId.value,
    helpDeskId: helpDeskId.value
  });

  helpDeskService.getAllHelpDesks(payload).then((resp) => {
    rows.value = resp.data;
    const displayTicketNo = resp.data?.[0]?.displayTicketNo;
    if (displayTicketNo) {
      prefixRef.value = displayTicketNo.split("-")[0];
    }
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

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions
// ----------------------------------------------------------------------------------------------------------------

const refreshHelpDeskList = () => {
  getAllHelpDesks({ pagination: pagination.value });
};

const refreshAllUserListByRoleForDropdown = () => {
  supportTeamUserForDropdown.load("support team");
}

const refreshGetHelpDesk = () => {
  getHelpDesk(helpDeskId.value);
}

// handleHelpDeskClick
const onHelpDeskSelect = async (helpDesk) => {
  if (!helpDesk?.id) return;
  isSearchTriggered.value = false;
  selectedHelpDeskId.value = helpDesk.id;

  helpDeskId.value = helpDesk.id;

  saveDataTableState({
    search: search.value,
    pagination: pagination.value,
    activeRowId: activeRowId.value,
    helpDeskId: helpDesk.id
  });

  loggedInEmail.value = helpDesk.employee?.person?.primaryEmailAddress || null;
  employeeId.value = helpDesk.employee?.id || null;
  twilioEmailId.value = helpDesk.twilioEmailId || null;
  emailSubject.value = helpDesk.title || null;
};

const getHelpDesk = (id) => {
  loading.value = true;
  helpDeskService.getHelpDesk(id).then((resp) => {
    model.value = _.cloneDeep(resp);
    const validateAssignedToId = model.value?.assignedToId ?? "";
    isAssignedToCurrentUser.value = validateAssignedToId === currentUserId;
  }).finally(() => {
    loading.value = false;
  });
};

function onSaveComment (id, comment) {
  setTimeout(function () {
    const payload = {
      closingComment: comment
    };
    helpDeskService.addorUpdateHelpDeskStatusComment(id, payload).then(resp => {
      notifySuccess({ message: "Ticket comment is saved successfully." });
      refreshHelpDeskList();
    });
  });
}

const onReplyToggle = () => {
  if (showReplyEditor.value && replyText.value.trim()) {
    $q.dialog({
      title: "Discard reply?",
      message: "You have unsent text. Are you sure you want to close?",
      cancel: true,
      persistent: true
    }).onOk(() => {
      replyText.value = "";
      showReplyEditor.value = false;
    });
  } else {
    showReplyEditor.value = !showReplyEditor.value;
  }
};

const resetSelectedHelpDesk = () => {
  helpDeskId.value = null;
  model.value.displayTicketNo = null; // controls Ticket Attributes visibility
  emailKey.value++;
};

const onAddNote = (id, type, moduleId, module, name, isShow) => {
  $q.dialog({
    component: addNote,
    componentProps: { id, type, moduleId, module, name, isShow }
  }).onOk(() => {
    refreshHelpDeskList();
  }).onCancel(() => {
    refreshHelpDeskList();
  }).onDismiss(() => {
  });
};

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------

initHelpDeskDialogs(activeRowId);
initPersonDialogs(activeRowId);
initHelpDeskActions(activeRowId);

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear for project
// --------------------------------------------------------------------------------------------------------------------------------------------------

const onAdvanceSearch = () => {
  isSearchTriggered.value = true;
  resetSelectedHelpDesk();
  refreshHelpDeskList();
};

const onAdvanceClear = () => {
  search.value.searchText = "";
  search.value.employeeEmails = [];
  search.value.ticketNo = 0;
  search.value.title = "";
  search.value.companyIds = [];
  search.value.categoryIds = [];
  search.value.statusIds = [];
  search.value.priorityIds = [];
  search.value.topicIds = [];
  search.value.questionIds = [];
  search.value.ticketFromDate = "";
  search.value.ticketToDate = "";
  search.value.assignedToId = "";

  isSearchTriggered.value = true;
  resetSelectedHelpDesk();
  saveDataTableState({
    search: search.value
  });
  onAdvanceSearch();
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Advance Filter:- Applied Filter Labels.
// --------------------------------------------------------------------------------------------------------------------------------------------------

const mapFilterToLabel = (ids, list, label) => {
  if (!Array.isArray(ids) || !ids.length) return {};

  const text = ids
    .map(id => {
      const match = list.value.find(item => item.value === id);
      return match ? match.text : id;
    })
    .join(", ");

  return { [label]: text };
};

const appliedFilters = computed(() => ({
  ...(search.value.title ? { Title: search.value.title } : {}),
  ...(search.value.ticketNo ? { "Ticket No": prefixRef.value + "-" + search.value.ticketNo } : {}),
  ...(search.value.assignedToId ? { "Assigned To": search.value.assignedToId } : {}),
  ...mapFilterToLabel(search.value.employeeEmails, requesterNameForDropdown.list, "Requester"),
  ...mapFilterToLabel(search.value.companyIds, helpDeskCompanyList, "Company Name"),
  ...mapFilterToLabel(search.value.statusIds, helpDeskStatusDropdown.list, "Status"),
  ...mapFilterToLabel(search.value.priorityIds, helpDeskPriorityDropdown.list, "Priority"),
  ...mapFilterToLabel(search.value.topicIds, helpDeskActiveWorkspaceDropdown.list, "Workspace"),
  ...mapFilterToLabel(search.value.questionIds, helpDeskMenusDropdown.list, "Menu"),
  ...mapFilterToLabel(search.value.categoryIds, helpDeskCategoryDropdown.list, "Category"),
  ...(search.value.ticketFromDate ? { "From Date": search.value.ticketFromDate } : {}),
  ...(search.value.ticketToDate ? { "To Date": search.value.ticketToDate } : {})
}));

function onClearFilters (key) {
  if (key === "Requester") {
    search.value.employeeEmails = [];
  } else if (key === "Assigned To") {
    search.value.assignedToId = "";
  } else if (key === "Title") {
    search.value.title = "";
  } else if (key === "Ticket No") {
    search.value.ticketNo = 0;
  } else if (key === "From Date") {
    search.value.ticketFromDate = "";
  } else if (key === "To Date") {
    search.value.ticketToDate = "";
  } else if (key === "Company Name") {
    search.value.companyIds = [];
  } else if (key === "Category") {
    search.value.categoryIds = [];
  } else if (key === "Status") {
    search.value.statusIds = [];
  } else if (key === "Priority") {
    search.value.priorityIds = [];
  } else if (key === "Workspace") {
    search.value.topicIds = [];
  } else if (key === "Menu") {
    search.value.questionIds = [];
  }
  isSearchTriggered.value = true;
  resetSelectedHelpDesk();
  refreshHelpDeskList();
  saveDataTableState({
    search: search.value,
    pagination: pagination.value,
    helpDeskId: helpDeskId.value
  });
}

function getFilterCount (key) {
  switch (key) {
  case "Requester": return search.value.employeeEmails?.length || 0;
  case "Company Name": return search.value.companyIds?.length || 0;
  case "Category": return search.value.categoryIds?.length || 0;
  case "Status": return search.value.statusIds?.length || 0;
  case "Priority": return search.value.priorityIds?.length || 0;
  case "Workspace": return search.value.topicIds?.length || 0;
  case "Menu": return search.value.questionIds?.length || 0;
  default: return null; // For single-value filters like Year, Status
  }
}

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------

const {
  requesterNameForDropdown,
  helpDeskStatusDropdown,
  helpDeskPriorityDropdown,
  helpDeskActiveWorkspaceDropdown,
  helpDeskMenusDropdown,
  helpDeskCategoryDropdown,
  supportTeamUserForDropdown
} = helpDeskModule();

const customerList = ref([]);
const customerFilter = ref([]);

function getAllCustomerListForDropdown () {
  customerService.getAllCustomerListForDropdown().then((resp) => {
    const responseData = resp
      .map((item) => ({
        text: item.company
          ? item.company.name // Use Company name if available
          : `${item.person.firstName} ${item.person.lastName}`, // Otherwise, use Person name
        value: item.id
      }))
      .sort((a, b) => a.text.localeCompare(b.text));
    customerList.value = responseData;
    customerFilter.value = responseData;
  });
}

function getAllCustomerListFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      customerList.value = customerFilter.value;
    } else {
      customerList.value = customerFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
  // mark typed flag
  // projectTypedFlags.customer = val && val.length > 0;
}

// Get all company list from helpdesk for dropdown
const helpDeskCompanyList = ref([]);
const helpDeskCompanyListOptions = ref([]);
function getHelpDeskCompanyDropdown () {
  helpDeskService.getCompanyDropdown().then((resp) => {
    const responseData = resp
      .map((item) => ({ text: item.name, value: item.id }))
      .sort((a, b) => a.text.localeCompare(b.text));
    helpDeskCompanyList.value = responseData;
    helpDeskCompanyListOptions.value = responseData;

    // If only one company -> auto select
    // if (responseData.length === 1) {
    //   search.value.companyIds = [responseData[0].value];
    // } else {
    //   // multiple companies -> no auto select
    //   search.value.companyIds = [];
    // }
    // onSearch();
  });
}

function getAllHelpDeskCompanyListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      helpDeskCompanyList.value = helpDeskCompanyListOptions.value;
    } else {
      helpDeskCompanyList.value = helpDeskCompanyListOptions.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Ticket Attributes Dropdowns (GET)
// --------------------------------------------------------------------------------------------------------------------------------------------------

function getAllHelpDeskTopicList () {
  helpDeskTopicsQuestionsService.getAllHelpDeskTopicList().then((resp) => {
    helpDeskTopics.value = resp;
  });
}

function getStatusChangeLog (id, name, columnName) {
  $q.dialog({
    component: siteStatusLog,
    componentProps: { id, name, columnName }
  }).onOk(() => {
  }).onCancel(() => {
  }).onDismiss(() => {
  });
}

function getStatusColor (val) {
  if (val) {
    switch (val) {
    case "Open":
      return "primary";
    case "In Progress":
      return "orange";
    case "Completed":
      return "green";
    case "Closed":
      return "grey";
    default:
      return "grey";
    }
  }
}

function getPriorityColor (val) {
  if (val) {
    switch (val) {
    case "Urgent":
      return "deep-orange-5";
    case "High":
      return "orange";
    case "Medium":
      return "primary";
    default:
      return "grey";
    }
  }
}

// ===========================================================
// Status Flow
// ===========================================================

const STATUS_TRANSITIONS = {
  requester: {
    New: ["Cancelled"],
    Open: ["Cancelled"],
    "Awaiting Client": ["In Progress", "Completed"],
    Completed: ["Closed", "Reopen", "Cancelled"],
    Reopen: ["Cancelled"]
  },
  admin: {
    Open: ["Closed"],
    Assigned: ["In Progress", "Closed"],
    "In Progress": ["Awaiting Client", "Completed", "Closed"],
    "Awaiting Client": ["In Progress"],
    Completed: ["Closed"],
    Reopen: ["In Progress", "Closed"]
  },
  assignedEmployee: {
    Assigned: ["In Progress"],
    "In Progress": ["Completed", "Awaiting Client"],
    "Awaiting Client": ["In Progress"],
    Reopen: ["In Progress"]
  }
};

// Get current status text
function resolveStatusText (statusId) {
  return helpDeskStatusDropdown.list.value
    .find(s => s.value === statusId)
    ?.text ?? null;
}

function getVisibleStatusOptions (row) {
  const currentStatusText =
    row.statusText || resolveStatusText(row.statusId);

  if (!currentStatusText) {
    return helpDeskStatusDropdown.list.value;
  }

  const isAdmin = role === "admin";
  const isSupport = roleSupportTeam;
  const isRequester = row.requesterId === loginUserEmployeeId;

  let allowed = [];

  // Requester Flow
  if (isRequester) {
    allowed.push(
      ...(STATUS_TRANSITIONS.requester?.[currentStatusText] || [])
    );
  }

  // Admin Flow
  if (isAdmin) {
    allowed.push(
      ...(STATUS_TRANSITIONS.admin?.[currentStatusText] || [])
    );
  }

  // Support Team Flow
  if (isSupport) {
    let supportRole = "assignedEmployee";

    if (row.assignedToId !== loginUserEmployeeId) {
      supportRole = "admin";
    }

    allowed.push(
      ...(STATUS_TRANSITIONS[supportRole]?.[currentStatusText] || [])
    );
  }

  // Fallback for normal requester-only user
  if (!isAdmin && !isSupport && !isRequester) {
    allowed =
      STATUS_TRANSITIONS.requester?.[currentStatusText] ?? [];
  }

  // Remove duplicate statuses
  allowed = [...new Set(allowed)];

  // Return visible statuses
  return helpDeskStatusDropdown.list.value.filter(option => {
    // Always display current status
    if (option.text === currentStatusText) return true;

    // Display only allowed transitions
    return allowed.includes(option.text);
  });
}
// ===========================================================

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Ticket Attributes Dropdowns (Update)
// --------------------------------------------------------------------------------------------------------------------------------------------------
const withFormLoader = async (
  field,
  apiCall,
  successMessage = "Updated successfully.",
  errorMessage = "Update failed.",
  afterSuccess = null
) => {
  formLoading.value[field] = true;
  await nextTick();
  document.activeElement?.blur();

  try {
    await apiCall();
    notifySuccess({ message: successMessage });

    if (afterSuccess) {
      await afterSuccess();
    }
  } catch (error) {
    notifyError({ message: errorMessage });
  } finally {
    formLoading.value[field] = false;
  }
};

// change status
// const onChangeStatus = (id, statusId) => {
//   return withFormLoader(
//     "status",
//     () => helpDeskService.updateHelpDeskStatus(id, statusId),
//     "Ticket status updated successfully.",
//     "Failed to update status.",
//     async () => {
//       getHelpDesk(helpDeskId.value);
//       refreshHelpDeskList();
//     }
//   );
// };

// // change priority
// const onChangePriority = (id, priorityId) => {
//   return withFormLoader(
//     "priority",
//     () => helpDeskService.updateHelpDeskPriority(id, priorityId),
//     "Priority updated successfully.",
//     "Failed to update priority.",
//     async () => {
//       getHelpDesk(helpDeskId.value);
//       refreshHelpDeskList();
//     }
//   );
// };

// // change assignedTo
// const onChangeAssignedTo = (id, assignedToId) => {
//   return withFormLoader(
//     "assignedTo",
//     () => helpDeskService.updateAssignedTo(id, assignedToId),
//     "Assignment updated successfully.",
//     "Failed to update assignment.",
//     async () => {
//       getHelpDesk(helpDeskId.value);
//       refreshAllUserListByRoleForDropdown();
//     }
//   );
// };

// change company client
const onChangeCompany = (id, companyId) => {
  return withFormLoader(
    "company",
    () => helpDeskService.updateCompanyClient(id, companyId),
    "Company is saved successfully.",
    "Failed to update company.",
    () => getHelpDesk(helpDeskId.value)
  );
};

function isStatusDisabled (statusId) {
  const status = helpDeskStatusDropdown.list.value?.find(item => item.value === statusId)?.text;
  return status;
}

// ----------------------------
// Save static search into localstorage.
// ----------------------------

// Quick Search
watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  isSearchTriggered.value = true;
  resetSelectedHelpDesk();
  refreshHelpDeskList();
});

watch(
  () => search.value.searchText,
  () => {
    saveDataTableState({
      search: search.value,
      pagination: pagination.value,
      helpDeskId: helpDeskId.value
    });
  }
);

watch(() => search.value.topicIds, (newValue, oldValue) => {
  if (newValue !== oldValue) {
    search.value.questionIds = []; // Clear question dropdown
    helpDeskMenusDropdown.load(search.value.topicIds);
  }
}, { immediate: true });

watch(
  () => rows.value,
  (list) => {
    if (!list || list.length === 0) return;

    const stored = getTableState();

    // RESTORE FROM LOCAL STORAGE
    if (!isSearchTriggered.value && stored?.helpDeskId) {
      const match = list.find(x => x.id === stored.helpDeskId);
      if (match) {
        onHelpDeskSelect(match); // ALWAYS call
        return;
      }
    }

    // FIRST LOAD → auto select first
    if (!isSearchTriggered.value && !selectedHelpDeskId.value) {
      onHelpDeskSelect(list[0]);
    }
  },
  { immediate: true }
);

watch(helpDeskId, (val) => {
  if (val) {
    getHelpDesk(val);
    showReplyEditor.value = false;
  }
});

// watch(() => search.value.companyIds, (newVal) => {
//   if (newVal && newVal.length === 1) {
//     onSearch();
//   }
// },
// { deep: true }
// );

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On Page Load
// --------------------------------------------------------------------------------------------------------------------------------------------------

onMounted(async () => {
  if (tableState?.helpDeskId) {
    helpDeskId.value = tableState.helpDeskId;
  }
  refreshHelpDeskList();
  helpDeskPriorityDropdown.load("HelpDesk Priority");
  helpDeskStatusDropdown.load("HelpDesk Status");
  requesterNameForDropdown.load();
  helpDeskCategoryDropdown.load("HelpDesk Category");
  supportTeamUserForDropdown.load("support team");
  helpDeskActiveWorkspaceDropdown.load();
  getAllHelpDeskTopicList();
  getHelpDeskCompanyDropdown();
  getAllCustomerListForDropdown();
});

</script>
<style>
.box-shadow{
  border-radius: 4px;
  box-shadow: 0 1px 5px rgba(0, 0, 0, 0.2), 0 2px 2px rgba(0, 0, 0, 0.14), 0 3px 1px -2px rgba(0, 0, 0, 0.12);
}
.editorImages img {
  width: 100%;
}
.white-border-checkbox .q-checkbox__inner {
  color: white !important;          /* unchecked icon */
}
.white-border-checkbox
  .q-checkbox__inner--truthy
  .q-checkbox__svg {
  color: rgb(7, 87, 119) !important;          /* tick color */
}
.white-border-checkbox
  .q-checkbox__inner--truthy
  .q-checkbox__bg {
  background: white !important;     /* checked box fill */
}
</style>
