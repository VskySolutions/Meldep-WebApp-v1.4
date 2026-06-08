<template>
  <q-page padding @update-calendar="updateCalendarData">
    <q-card class="breadcrumSection project6 flex justify-between items-center">
      <!-- Breadcrumb Section -->
      <q-card-section class="card-header with-tools flex justify-between items-center">
        <q-breadcrumbs class="text-brown text-weight-bold text-h3">
          <template #separator>
            <q-icon size="1.5em" name="o_chevron_right" color="primary" />
          </template>
          <q-breadcrumbs-el label="Settings" />
          <q-breadcrumbs-el label="Leave Rules" clickable to="/leave-rules" />
          <q-breadcrumbs-el label="Leave Schedule" />
        </q-breadcrumbs>
      </q-card-section>
      <q-card-section class="flex items-center no-padding">
        <q-btn icon="o_add" outline label="Weekly Saturday's Off" no-caps class="text-primary btnRounded q-mr-md" @click="onAdd" />
        <q-btn icon="o_chevron_left" outline label="Back" no-caps class="text-primary btnRounded q-mr-md no-space-between" @click="$router.back()" />
      </q-card-section>
    </q-card>
    <q-card class="items-center">
      <div class="app-main q-mt-md">
        <div class="div q-col-gutter-x-md q-mb-md q-pb-sm q-pt-md">
          <div class="col-lg-10 col-md-10 col-sm-9 col-xs-6 q-mb-md" style="margin-left: 5%; margin-right: 5%;">
            <div class="">
              <FullCalendar ref="calsession" class="demo-app-calendar" :options="calendarOptions">
                <template #eventContent="arg">
                  <b>{{ arg.timeText }}</b>
                  <i class="cursor-pointer event-title">{{ truncateText(arg.event.title, 30) }}</i>
                </template>
              </FullCalendar>
            </div>
          </div>
        </div>
      </div>
    </q-card>
  </q-page>
</template>

<script setup>
import { ref, watch, onMounted } from "vue";
import { useQuasar } from "quasar";
import FullCalendar from "@fullcalendar/vue3";
import dayGridPlugin from "@fullcalendar/daygrid";
import timeGridPlugin from "@fullcalendar/timegrid";
import interactionPlugin from "@fullcalendar/interaction";
import listPlugin from "@fullcalendar/list";
import bootstrap5Plugin from "@fullcalendar/bootstrap5";
import eventLeaveService from "modules/leave-yearly-schedule/leaveYearlySchedule.service";
import addLeaveSchedule from "modules/leave-yearly-schedule/components/addEdit.vue";
import addEditEvents from "modules/leave-yearly-schedule/components/addEditEvents.vue";
import useFilters from "composables/useFilters";
import ShowEvent from "modules/leave-yearly-schedule/components/view.vue";
import _ from "lodash";
import { useRoute } from "vue-router";

const $q = useQuasar();
const loading = ref(true);
const { toDate } = useFilters();
const { randomIntFromInterval } = useFilters();
const route = useRoute();
const selectedYear = ref(new Date().getFullYear()); // Default to current year
const selectedRuleId = ref(route.query.leaveRuleId); // stored ruleId
const currentEvents = ref([]);
const INITIAL_EVENTS = [];
const eventColor = ["#e9edc9", "#fefae0", "#fae1dd", "#d6e2e9", "#fde2e4", "#FFFAFA"];

onMounted(() => {
  getAllActiveCalendarClasses();
});

// Create popup
const onAdd = () => {
  $q.dialog({
    component: addLeaveSchedule,
    componentProps: {
      leaveRuleId: selectedRuleId.value, // Pass leaveRuleId
      selectedYear: selectedYear.value
    }
  }).onOk(() => {
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

const calendarData = ref({
  title: ""
});

const getAllActiveCalendarClasses = () => {
  const INITIAL_EVENTS = [];
  loading.value = true;
  const payload = {}; // No filters, fetch all events

  eventLeaveService.getLeaveEvents(payload).then((resp) => {
    calendarData.value = _.cloneDeep(resp);
    resp.forEach(element => {
      const daysOfWeekArray = [];
      if (Array.isArray(element.classDayMappings)) {
        element.classDayMappings.forEach(dayMapping => {
          daysOfWeekArray.push(dayMapping.day.sortorder);
        });
      }

      if (element.date) {
        INITIAL_EVENTS.push({
          id: element.id,
          overlap: false,
          textColor: "black",
          color: eventColor[randomIntFromInterval(0, 5)],
          title: element.title,
          date: toDate(element.date, "YYYY-MM-DD"),
          event_type: "C"
        });
      } else {
        INITIAL_EVENTS.push({
          id: element.id,
          overlap: false,
          textColor: "black",
          title: element.title,
          color: eventColor[randomIntFromInterval(0, 5)],
          daysOfWeek: daysOfWeekArray,
          event_type: "C"
        });
      }
    });
  }).finally(() => {
    loading.value = false;
    calendarOptions.value.events = INITIAL_EVENTS; // Update FullCalendar
  });
};

const handleEventClick = (clickInfo) => {
  const id = clickInfo.event.id;
  const eventType = clickInfo.event.extendedProps.event_type;
  $q.dialog({
    component: ShowEvent,
    componentProps: { id, eventType }
  }).onOk(() => {
    getAllActiveCalendarClasses();
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

const handleEvents = (events) => {
  currentEvents.value = events;
};

// const calendarOptions = ref({
//   plugins: [
//     dayGridPlugin,
//     timeGridPlugin,
//     interactionPlugin,
//     listPlugin,
//     bootstrap5Plugin
//   ],
//   headerToolbar: {
//     left: "prev,next today",
//     center: "title",
//     right: "dayGridMonth,listMonth"
//     // right: "dayGridMonth,timeGridWeek,timeGridDay"
//   },
//   // nextDayThreshold: "09:00:00",
//   showNonCurrentDates: false,
//   fixedWeekCount: false,
//   displayEventTime: false,
//   eventLimit: true,
//   eventDisplay: "block",
//   initialView: "dayGridMonth",
//   height: "80vh",
//   contentHeight: "80vh", // Prevents internal scroll
//   expandRows: true, // Ensures all weeks have the same height
//   initialEvents: INITIAL_EVENTS,
//   // eventStartEditable: false,
//   // editable: true,
//   selectable: true,
//   selectMirror: true,
//   dayMaxEvents: true,
//   weekends: true,
//   disableDragging: true,
//   // select: handleDateSelect,
//   eventClick: handleEventClick,
//   // eventClick: openEditEventPopup,
//   eventsSet: handleEvents,
//   // eventMouseEnter: handleEventMouseEnter,
//   // Add dateClick event
//   dateClick: (info) => {
//     openAddEventPopup(info.dateStr);
//   },
//   initialDate: `${selectedYear.value}` // Default to selected year
// });

const targetMonth = ref(""); // Stores Target Month-Year

const calendarOptions = ref({
  plugins: [
    dayGridPlugin,
    timeGridPlugin,
    interactionPlugin,
    listPlugin,
    bootstrap5Plugin
  ],
  headerToolbar: {
    left: "prev,next today",
    center: "title",
    right: "dayGridMonth,listMonth"
  },
  showNonCurrentDates: false,
  fixedWeekCount: false,
  displayEventTime: false,
  eventLimit: true,
  eventDisplay: "block",
  initialView: "dayGridMonth",
  height: "80vh",
  contentHeight: "80vh",
  expandRows: true,
  selectable: true,
  selectMirror: true,
  dayMaxEvents: true,
  initialEvents: INITIAL_EVENTS,
  weekends: true,
  disableDragging: true,
  eventClick: handleEventClick,
  eventsSet: handleEvents,
  dateClick: (info) => {
    openAddEventPopup(info.dateStr);
  },
  initialDate: `${selectedYear.value}-01-01`, // Default Year
  datesSet: (info) => {
    updateSelectedMonth(info.view.title); // Update Month-Year on view change
  }
});

const updateSelectedMonth = (title) => {
  const date = new Date(title); // Convert title to Date object
  const formattedDate = `${(date.getMonth() + 1).toString().padStart(2, "0")}/${date.getDate().toString().padStart(2, "0")}/${date.getFullYear()}`; // MM/DD/YYYY format
  targetMonth.value = formattedDate;
  localStorage.setItem("selectedMonthYear", formattedDate); // Store in Local Storage
};

// Restore last selected month from Local Storage on page load
onMounted(() => {
  const savedMonth = localStorage.getItem("selectedMonthYear");
  if (savedMonth) {
    targetMonth.value = savedMonth; // Load MM/DD/YYYY format
  }
});

watch(() => route.query, (query) => {
  if (query.year) {
    selectedYear.value = query.year;
    const currentDate = new Date();
    const currentYear = currentDate.getFullYear();
    const currentMonth = currentDate.getMonth() + 1; // Get current month (1-12)
    const displayMonth = (parseInt(query.year) === currentYear) ? currentMonth : 1;
    calendarOptions.value.initialDate = `${query.year}-${displayMonth.toString().padStart(2, "0")}-01`;
  }
  if (query.id) {
    selectedRuleId.value = query.id; // Store ruleId from query params
  }
}, { immediate: true });

const formatDate = (dateStr) => {
  const [year, month, day] = dateStr.split("-");
  return `${month}/${day}/${year}`;
};

const openAddEventPopup = (selectedDate) => {
  $q.dialog({
    component: addEditEvents,
    componentProps: {
      selectedDate: formatDate(selectedDate),
      leaveRuleId: selectedRuleId.value // Pass leaveRuleId
    }
  }).onOk(() => {
    getAllActiveCalendarClasses();
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

const truncateText = (text, maxLength) => {
  return text.length > maxLength ? text.substring(0, maxLength) + "..." : text;
};
</script>

<style>
.fc-list-table th {
  background-color: #2580b7 !important; /* Light primary color */
  color: white !important;
}
.fc-button-group .fc-button, .fc-today-button {
  background-color: #2580b7 !important;
}
</style>
