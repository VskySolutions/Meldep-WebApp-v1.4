<template>
  <div class="q-pa-md text-center q-pt-xl">
    <div class="header-text-line1">
      The
      <span class="text-primary">All-in-One</span>
      Platform. We make project
      <span class="text-primary">Organization,</span>
    </div>
    <div class="header-text-line2">
      <span class="text-primary">Planning and Tracking</span> smooth and effortless.
    </div>
  </div>

  <!-- Image Section -->
  <div class="flex flex-center bg-white">
    <q-img
      src="~assets/website/home/home1.png"
      style="height: auto; width: 100%; max-width: 1500px"
      spinner-color="primary"
      alt="Dashboard Overview"
    />
  </div>

  <!-- CTA Banner Section -->
  <div class="flex justify-center q-mb-lg">
    <div class="cta-container row items-center justify-between text-white">
      <!-- Text block -->
      <div class="cta-text col q-mx-lg">
        <div class="cta-title">Plan, Track, and Execute with Ease</div>
        <div class="cta-subtitle q-mb-md">
          Meldep simplifies project management with intuitive planning, tracking, and reporting<br>
          tools—all designed to keep your team productive and stress‑free.
        </div>
      </div>

      <!-- Button -->
      <div class="cta-button q-mx-lg">
        <q-btn
          unelevated
          label="Explore Meld‑Ep"
          color="white"
          text-color="black"
          size="md"
          class="text-capitalize q-px-lg q-py-sm shadow-1"
          style="border-radius: 7px; height: 45px; font-size: 16px"
        />
      </div>
    </div>
  </div>

  <div class="row q-pa-xl items-start q-col-gutter-sm">
    <!-- 🟦 Carousel (left) -->
    <div class="col-5 carousel-container q-pt-lg">
      <q-carousel
        v-model="slide"
        control-color="primary"
        :autoplay="autoplay"
        infinite
        interval="1000"
        class="custom-carousel"
      >
        <q-carousel-slide name="0">
          <q-img
            src="~assets/website/Home/home2.png"
            class="full-width custom-img"
            style="width: 90%; height: 90%; object-fit: cover"
          />
        </q-carousel-slide>
        <q-carousel-slide name="1">
          <q-img
            src="~assets/website/Home/home3.png"
            class="full-width custom-img"
            style="width: 90%; height: 90%; object-fit: cover"
          />
        </q-carousel-slide>
        <q-carousel-slide name="2">
          <q-img
            src="~assets/website/Home/home4.png"
            class="full-width custom-img"
            style="width: 90%; height: 90%; object-fit: cover"
          />
        </q-carousel-slide>
        <q-carousel-slide name="3">
          <q-img
            src="~assets/website/Home/home5.png"
            class="full-width custom-img"
            style="width: 90%; height: 90%; object-fit: cover"
          />
        </q-carousel-slide>
        <q-carousel-slide name="4">
          <q-img
            src="~assets/website/Home/home6.png"
            class="full-width custom-img"
            style="width: 90%; height: 90%; object-fit: cover"
          />
        </q-carousel-slide>
        <q-carousel-slide name="5">
          <q-img
            src="~assets/website/Home/home7.png"
            class="full-width custom-img"
            style="width: 90%; height: 90%; object-fit: cover"
          />
        </q-carousel-slide>
      </q-carousel>

      <div class="custom-div hide-on-mobile" />
    </div>

    <!-- 🟨 Dynamic Content (middle) -->
    <div class="col-5">
      <div class="q-pt-none">
        <div v-if="selected === 'planning'">
          <div class="q-px-xl">
            <div class="text-weight-bold" style="color: #1b75ab; font-size: 38px">
              Project Planning
            </div>

            <div class="text-h5 text-weight-bold">Clear Scheduling, No Last‑Minute Rush</div>

            <div class="text-body1 q-mt-sm">
              Plan daily, weekly, and monthly tasks with structured scheduling tools. Assign work
              under specific projects, adjust plans based on workload, and ensure every task is
              accounted for. Automatic scheduling prevents conflicts, keeping projects on track
              without last-minute adjustments.
            </div>

            <div class="q-mt-md">
              <div class="text-body1 q-mb-sm">
                <span class="text-weight-bold">Daily Planner –</span>
                Plan your workday effortlessly by assigning tasks under projects and work orders.
              </div>

              <div class="text-body1 q-mb-sm">
                <span class="text-weight-bold">Weekly Project Plan –</span>
                Get a clear view of upcoming tasks and ensure alignment across teams.
              </div>

              <div class="text-body1">
                <span class="text-weight-bold">Monthly Planner –</span>
                Schedule tasks in advance and auto‑adjust for team availability.
              </div>
            </div>
          </div>
        </div>

        <div v-else-if="selected === 'management'">
          <div class="q-px-xl">
            <div class="text-weight-bold" style="color: #1b75ab; font-size: 38px">
              Project Management & Tracking
            </div>

            <div class="text-h5 text-weight-bold">One Dashboard, Full Visibility</div>

            <div class="text-body1 q-mt-sm">
              Monitor project timelines, assigned work, and overall progress in a single view. Task
              details, timesheets, and billing data are all consolidated, reducing dependency on
              external trackers. Stay informed on where projects stand without switching between
              multiple tools.
            </div>

            <div class="q-mt-md">
              <div class="text-body1 q-mb-sm">
                <span class="text-weight-bold">Project Overview –</span>
                Instantly see task assignments, deadlines, and progress.
              </div>

              <div class="text-body1">
                <span class="text-weight-bold">Real‑Time Tracking –</span>
                No more scattered data—track timesheets, tasks, and billing from a single interface.
              </div>
            </div>
          </div>
        </div>

        <div v-else-if="selected === 'reporting'">
          <div class="q-px-xl">
            <div class="text-weight-bold" style="color: #1b75ab; font-size: 38px">
              Project Reporting
            </div>

            <div class="text-h5 text-weight-bold">Work Hours and Finances in Check</div>

            <div class="text-body1 q-mt-sm">
              Integrated timesheets allow precise logging of worked hours, ensuring accurate payroll
              processing and performance tracking. Client and project accounting store all financial
              interactions, from initial engagement to final invoicing, keeping cost management
              transparent and structured.
            </div>

            <div class="q-mt-md">
              <div class="text-body1 q-mb-sm">
                <span class="text-weight-bold">Timesheets -</span>
                Log daily hours effortlessly for precise payroll and performance tracking.
              </div>

              <div class="text-body1 q-mb-sm">
                <span class="text-weight-bold">Client & Project Accounting -</span>
                Manage financials from lead acquisition to final billing, keeping costs in check.
              </div>
            </div>
          </div>
        </div>

        <div v-else-if="selected === 'resources'">
          <div class="q-px-xl">
            <div class="text-weight-bold" style="color: #1b75ab; font-size: 38px">
              Resource Planning
            </div>

            <div class="text-h5 text-weight-bold">Balanced Workload, No Overbooking</div>

            <div class="text-body1 q-mt-sm">
              View team assignments, task priorities, and available resources in real time. The
              system automatically adjusts workloads based on approved leaves and working hours,
              distributing tasks efficiently to avoid overloading employees.
            </div>

            <div class="q-mt-md">
              <div class="text-body1 q-mb-sm">
                <span class="text-weight-bold">Task & Resource Management-</span>
                Get a clear picture of team capacity and prevent burnout.
              </div>

              <div class="text-body1 q-mb-sm">
                <span class="text-weight-bold">Smart Workload Adjustments - </span>
                Automatically redistribute tasks based on time- off and availability.
              </div>
            </div>
          </div>
        </div>

        <div v-else-if="selected === 'organization'">
          <div class="q-px-xl">
            <div class="text-weight-bold" style="color: #1b75ab; font-size: 38px">
              Organizational Management
            </div>

            <div class="text-h5 text-weight-bold">Secure Data, Defined Access</div>

            <div class="text-body1 q-mt-sm">
              Role-based access ensures that only authorized personnel can modify or manage specific
              modules. Critical project data remains protected, preventing unauthorized changes
              while maintaining operational efficiency.
            </div>

            <div class="q-mt-md">
              <div class="text-body1 q-mb-sm">
                <span class="text-weight-bold">Employee & Role-Based Permissions -</span>
                Define who can view, edit, and manage sensitive information.
              </div>

              <div class="text-body1 q-mb-sm">
                <span class="text-weight-bold">Data Security - </span>
                Prevent unauthorized changes and maintain business confidentiality.
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    <q-container>
      <!-- Sidebar -->
      <div class="col-2 row justify-center q-pl-xl">
        <div class="rounded-lg q-pa-sm list q-py-md" style="max-width: 240px">
          <q-list class="rounded-borders q-pt-md" style="margin-left: -35px">
            <q-item
              v-for="item in menu"
              :key="item.value"
              v-ripple
              clickable
              :active="selected === item.value"
              active-class="active"
              class="q-mb-sm rounded-borders"
              @click="selected = item.value"
            >
              <q-item-section avatar>
                <!-- <div class="bg-white shadow-4 icon"> -->
                <q-icon
                  :name="item.icon"
                  class="shadow-4 icon"
                  :style="selected === item.value ? 'background-color: #4b8eb7; color: white;' : ''"
                />
                <!-- </div> -->
              </q-item-section>
              <q-item-section>
                <q-item-label class="text-weight-medium label">{{ item.label }}</q-item-label>
              </q-item-section>
            </q-item>
          </q-list>
        </div>
      </div>
    </q-container>
  </div>

  <div class="q-px-xl q-pt-xl q-pb-xl" style="background-color: #ecf1f5">
    <div class="container">
      <q-container>
        <div class="row justify-between q-ml-md">
          <!-- Left Column -->
          <div class="col-12 col-md-6 q-pl-xl q-mx-xl">
            <div class="text-left text1">
              <h4 class="text-weight-bold q-mb-md" style="color: #1b75ab; font-size: 2rem">
                Why Choose Meld-Ep?
              </h4>
              <p class="text-weight-medium" style="font-size: 14px; line-height: 1.6">
                We simplify project management with smart planning tools that keep your team
                aligned, your timelines on track, and your data organized — all in one place.<br>
                From task planning to timesheets, MeldEp helps you work faster, bill smarter, and
                deliver better.
              </p>
            </div>
          </div>

          <!-- Right Column -->
          <div class="col-12 col-md-4 q-mb-xl q-pl-sm">
            <q-card
              class="hover-card shadow-5 transition-all flex flex-center responsive-card"
              style="max-width: 400px; margin: auto"
            >
              <div class="text-body2 text-center text-weight-medium">
                Enable only the features you need – Whether you require Project Management,
                Organization Management, HR, Talent Acquisition, SDLC, Inventory or CRM you can
                configure MeldEp to match your business requirements.
              </div>
            </q-card>
          </div>
        </div>
      </q-container>

      <!-- Bottom Section: Feature Cards -->
      <q-container class="q-pt-md q-mb-xl">
        <div class="row q-col-gutter-md justify-center">
          <!-- Card 1 -->
          <div class="col-12 col-md-6 col-lg-4 q-mb-md card2">
            <q-card class="hover-card shadow-2 transition-all flex flex-center responsive-card">
              <div class="text-body2 text-center text-weight-medium">
                No more double entries – All modules are fully integrated, allowing seamless data
                flow across different functionalities. Users can start working immediately without
                redundant data entry or additional setup.
              </div>
            </q-card>
          </div>

          <!-- Card 2 -->
          <div class="col-12 col-md-6 col-lg-4 q-mb-md">
            <q-card
              class="hover-card shadow-2 transition-all flex flex-center responsive-card"
              style="max-width: 400px; margin: auto"
            >
              <div class="text-body2 text-center text-weight-medium">
                Customizable and extensible – MeldEp adapts to your business needs. It can be
                tailored and extended to support your specific requirements, allowing businesses to
                scale efficiently without limitations.
              </div>
            </q-card>
          </div>

          <!-- Card 3 -->
          <div class="col-12 col-md-6 col-lg-4 q-mb-md card4">
            <q-card
              class="hover-card shadow-2 transition-all flex flex-center responsive-card"
              style="max-width: 400px; margin: auto"
            >
              <div class="text-body2 text-center text-weight-medium">
                Upgrade anytime without disruption – MeldEp’s development process ensures that all
                upgrades are integrated by design. No matter which version you are using, you can
                upgrade to the latest version without losing data, ensuring continuous access to new
                features.
              </div>
            </q-card>
          </div>
        </div>
      </q-container>
    </div>
  </div>

  <div class="bg-black text-white q-py-xl q-px-md">
    <q-container class="q-px-none">
      <div class="column items-center justify-center text-center">
        <div
          class="q-mb-md text-weight-light text-body1 text-md-subtitle1 text-lg-subtitle1 demo-section"
          style="max-width: 1400px; margin: 0 auto"
        >
          MeldEp is built by business and development experts with 30+ years of experience. More
          than just a tool, it was <br class="gt-sm">
          created by our team, for our team. Our experts will personally guide you through the
          setup, process, and <br class="gt-sm">
          training, showing you exactly how we use it, so you can do the same.
        </div>

        <q-btn
          label="Book a Demo"
          color="white"
          text-color="black"
          class="q-mt-lg text-weight-bold"
          size="lg"
          unelevated
          style="
            font-size: 16px;
            height: 50px;
            width: 180px;
            border-radius: 7px;
            text-transform: none;
          "
          @click="$router.push('/demo')"
        />
      </div>
    </q-container>
  </div>

  <div class="q-pa-xl bg-white q-mx-lg">
    <div class="text-center q-mb-xl">
      <h4 class="text-weight-bold" style="color: #1b75ab; font-size: 40px">
        Frequently Asked Questions
      </h4>
    </div>

    <div class="row q-col-gutter-xl">
      <!-- FAQ Accordion Section -->
      <div class="col-12 col-lg-7">
        <q-expansion-item
          v-for="(faq, index) in faqs"
          :key="index"
          v-model="faq.expanded"
          class="shadow-6 rounded-borders q-mb-md"
          hide-expand-icon
          style="margin: 20px auto; width: 85%; max-width: 85%; border: 1px solid #ccc"
        >
          <!-- border: 1px solid gray -->
          <template #header>
            <div
              class="q-pl-lg q-pr-lg font-weight-medium row items-center justify-between"
              style="width: 100%"
            >
              <span class="text-body1">{{ faq.question }}</span>

              <div
                class="q-pa-xs rounded-full cursor-pointer"
                style="
                  transition: transform 0.3s;
                  width: 20px;
                  height: 20px;
                  display: flex;
                  justify-content: center;
                  align-items: center;
                  border-radius: 50%;
                  background-color: rgba(27, 117, 171, 0.6);
                  color: #1b75ab;
                "
              >
                <q-icon name="o_expand_more" :class="{ 'rotate-180': faq.expanded }" size="12px" />
              </div>
            </div>
          </template>

          <div class="q-pa-sm text-grey-8 q-pl-xl">
            {{ faq.answer }}
          </div>
        </q-expansion-item>

        <!-- Pagination -->
        <div class="row justify-center q-mt-md">
          <q-btn
            icon="o_chevron_left"
            round
            color="white"
            flat
            size="11px"
            class="q-mr-lg q-mt-sm"
            style="background-color: #1b75ab"
          />
          <q-btn
            icon="o_chevron_right"
            round
            color="white"
            flat
            size="11px"
            class="q-ml-lg q-mt-sm"
            style="background-color: #1b75ab"
          />
        </div>
      </div>

      <!-- Side Image + Form Section -->
      <div class="col-12 col-lg-5 q-mt-lg q-mt-lg-none">
        <div class="column items-center justify-center question">
          <img
            src="~assets/website/Home/Question.png"
            alt="Any Questions?"
            style="max-width: 220px; width: 100%"
            class="q-mb-sm"
          >
          <div class="text-h5 text-bold text-center q-mb-sm">Any Questions?</div>
          <div class="full-width q-pa-sm">
            <label class="text-black q-ml-xs q-mb-xs text-weight-regular" style="font-size: 14px">Let us Know</label>
            <q-input
              placeholder="Enter Here"
              class="shadow-4 rounded-borders bg-white q-mt-sm"
              input-class="q-pa-md"
              style="border: 1px solid #ccc"
            />
          </div>
          <q-btn
            label="Send"
            class="q-mt-lg q-px-xl shadow-5"
            size="md"
            unelevated
            style="
              background-color: #1b75ab;
              color: white;
              border-radius: 10px;
              text-transform: none;
              font-size: 18px;
            "
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from "vue";

const slide = ref("0");
const autoplay = ref(true);

const selected = ref("planning");

const menu = [
  { label: "Project Planning", value: "planning", icon: "o_calendar_month" },
  { label: "Project Management & Tracking", value: "management", icon: "o_view_list" },
  { label: "Project Reporting", value: "reporting", icon: "o_insert_chart" },
  { label: "Resource Planning", value: "resources", icon: "o_settings" },
  { label: "Org Management", value: "organization", icon: "o_domain" }
];

const faqs = [
  {
    question: "What is MeldEp, and how does it help in project management?",
    answer: "MeldEp simplifies project workflows with integrated modules..."
  },
  {
    question: "Can I enable only the modules I need?",
    answer: "Yes, you can enable only the specific modules needed..."
  },
  {
    question: "Is there any data duplication between modules?",
    answer: "No, the modules share a unified database structure..."
  },
  {
    question: "How does MeldEp handle system upgrades?",
    answer: "Upgrades are seamless with built-in version control..."
  },
  {
    question: "Can MeldEp be customized for my business needs?",
    answer: "Absolutely, the platform is modular and extensible..."
  }
];
</script>
