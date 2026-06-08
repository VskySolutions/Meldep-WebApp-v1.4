<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-md-3">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Org Management" />
              <q-breadcrumbs-el label="Org Structure" />
              <q-breadcrumbs-el label="Preview" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-md-9 ActionButtons text-end">
            <q-btn icon="o_chevron_left" outline label="Back" no-caps class="text-primary q-ml-sm" @click="$router.back()">
              <q-tooltip anchor="bottom middle" self="top middle">Back To List</q-tooltip>
            </q-btn>
          </div>
        </div>
        <div class="row q-pt-lg q-pa-xs">
          <div class="org-chart-page">
            <div class="toolbar">
              <div class="search-box">
                <input
                  v-model="searchText"
                  type="search"
                  placeholder="Search by name"
                  @input="filterChart"
                >
              </div>

              <div class="toolbar-actions">
                <button @click="expandAll">Expand All</button>
                <button @click="collapseAll">Collapse All</button>
                <button @click="fitChart">Fit</button>
                <button @click="clearSearch">Clear</button>
              </div>
            </div>

            <div ref="chartContainer" class="chart-container" />
          </div>
        </div>
      </q-card-section>
      <q-separator />
    </q-card>
  </q-page>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount } from "vue";
import { OrgChart } from "d3-org-chart";
import * as d3 from "d3";
import orgStructureService from "modules/employee-org-structure/employeeOrgStructure.service";

const chartContainer = ref(null);
const searchText = ref("");

let chart = null;
let chartData = [];

async function loadOrgData () {
  const response = await orgStructureService.getEmployeeOrgStructurePreview(2026);
  chartData = (response ?? []).map((item) => ({
    id: item.id,
    parentId: item.parentId === undefined ? null : item.parentId,
    name: item.name,
    position: item.position,
    department: item.department,
    responsibilities: item.responsibilities,
    color: item.color,
    image: item.image,
    sortOrder: item.sortOrder
  }));
  renderChart(chartData);
}

function renderChart (data) {
  if (!chartContainer.value) return;

  chart = new OrgChart()
    .container(chartContainer.value)
    .data(data)
    .nodeHeight(() => 130)
    .nodeWidth(() => 230)
    .childrenMargin(() => 35)
    .compactMarginBetween(() => 25)
    .compactMarginPair(() => 20)
    .neighbourMargin(() => 15)
    .initialZoom(0.9)
    .buttonContent(({ node, state }) => {
      return `
        <div style="
          width:24px;
          height:24px;
          border-radius:50%;
          background:#fff;
          border:1px solid #d9d9d9;
          display:flex;
          align-items:center;
          justify-content:center;
          font-size:14px;
          color:#444;
          box-shadow:0 2px 6px rgba(0,0,0,0.08);
        ">
          ${node.children ? "−" : "+"}
        </div>
      `;
    })
    .linkUpdate(function (d) {
      d3.select(this)
        .attr("stroke", "#c1c1c1")
        .attr("fill", "none");
    })
    .nodeContent((d) => {
      const borderColor = d.data.color || "#E4E2E9";
      const imageUrl = d.data.image || "";
      const safeName = escapeHtml(d.data.name || "");
      const safePosition = escapeHtml(d.data.position || "");
      const safeDepartment = escapeHtml(d.data.department || "");
      const safeResponsibilities = escapeHtml(d.data.responsibilities || "");

      const isHighlighted = d.data._highlighted;
      const cardBorder = isHighlighted ? "#2F80ED" : "#E4E2E9";
      const cardShadow = isHighlighted
        ? "0 0 0 2px rgba(47,128,237,0.18), 0 8px 20px rgba(47,128,237,0.18)"
        : "0 4px 14px rgba(15, 23, 42, 0.08)";

      return `
        <div style="
          width:${d.width}px;
          height:${d.height}px;
          padding-top:22px;
          padding-left:1px;
          padding-right:1px;
          box-sizing:border-box;
        ">
          <div style="
            position:relative;
            font-family:Inter, Arial, sans-serif;
            background:#ffffff;
            width:${d.width - 2}px;
            height:${d.height - 22}px;
            border-radius:14px;
            border:1px solid ${cardBorder};
            box-shadow:${cardShadow};
            box-sizing:border-box;
          ">
            <div style="
              position:absolute;
              left:0;
              top:0;
              bottom:0;
              width:6px;
              background:${borderColor};
              border-top-left-radius:14px;
              border-bottom-left-radius:14px;
            "></div>

            <div style="
              display:flex;
              margin-left: 75px;
              padding:10px 12px 0 12px;
              font-size:15px;
              color:#1b75ab;
              font-weight: 600;
            ">
              ${safeName}
            </div>

            <div style="
              position:absolute;
              top:-18px;
              left:18px;
              width:54px;
              height:54px;
              border-radius:50%;
              background:#fff;
              display:flex;
              align-items:center;
              justify-content:center;
              box-shadow:0 2px 8px rgba(0,0,0,0.08);
              border:1px solid #eef0f4;
              overflow:hidden;
            ">
              <img
                src="${imageUrl}"
                alt="${safeName}"
                style="width:46px;height:46px;border-radius:50%;object-fit:cover;"
              />
            </div>

            <div style="padding:10px">

              <div style="
                margin-top:5px;
                font-size:10px;
                color:#475467;
                background:#F5F7FA;
                border-radius:999px;
                padding:4px 8px;
                max-width:100%;
                white-space:nowrap;
                overflow:hidden;
                text-overflow:ellipsis;
              ">
                <b>Department:-</b> ${safeDepartment}
              </div>

              <div style="
                margin-top:5px;
                font-size:10px;
                color:#475467;
                background:#F5F7FA;
                border-radius:999px;
                padding:4px 8px;
                max-width:100%;
                white-space:nowrap;
                overflow:hidden;
                text-overflow:ellipsis;
              ">
                <b>Designation:-</b> ${safePosition}
              </div>

              <div
                title="${safeResponsibilities}"
                style="
                  margin-top:8px;
                  font-size:10px;
                  color:#7A7F87;
                  line-height:1.35;
                  display:-webkit-box;
                  -webkit-line-clamp:2;
                  -webkit-box-orient:vertical;
                  overflow:hidden;
                  min-height:27px;
                "
              >
                ${safeResponsibilities}
              </div>
            </div>
          </div>
        </div>
      `;
    });

  chart.render();
  chart.expandAll();
}

function filterChart () {
  if (!chart) return;

  const value = (searchText.value || "").trim().toLowerCase();
  const data = chart.data();

  chart.clearHighlighting();

  data.forEach((d) => {
    d._highlighted = false;
    d._expanded = true;
  });

  if (value) {
    data.forEach((d) => {
      const name = (d.name || "").toLowerCase();
      const position = (d.position || "").toLowerCase();
      const department = (d.department || "").toLowerCase();

      if (
        name.includes(value) ||
        position.includes(value) ||
        department.includes(value)
      ) {
        d._highlighted = true;
        d._expanded = true;

        // expand manager chain
        expandParentChain(d, data);
      }
    });
  }

  chart.data(data).render().fit();
}

function expandParentChain (node, data) {
  let currentParentId = node.parentId;

  while (currentParentId) {
    const parent = data.find((x) => String(x.id) === String(currentParentId));
    if (!parent) break;

    parent._expanded = true;
    currentParentId = parent.parentId;
  }
}

function clearSearch () {
  searchText.value = "";

  if (!chart) return;

  const data = chart.data();
  chart.clearHighlighting();

  data.forEach((d) => {
    d._highlighted = false;
    d._expanded = true;
  });

  chart.data(data).render().fit();
}

function expandAll () {
  chart?.expandAll().fit();
}

function collapseAll () {
  chart?.collapseAll().fit();
}

function fitChart () {
  chart?.fit();
}

function escapeHtml (value) {
  return String(value)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll("\"", "&quot;")
    .replaceAll("'", "&#039;");
}

function handleResize () {
  chart?.fit();
}

onMounted(async () => {
  await loadOrgData();
  window.addEventListener("resize", handleResize);
});

onBeforeUnmount(() => {
  window.removeEventListener("resize", handleResize);
});
</script>

<style scoped>
.org-chart-page {
  width: 100%;
  height: 100%;
  box-sizing: border-box;
}

.toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  flex-wrap: wrap;
  margin-bottom: 14px;
}

.search-box {
  min-width: 280px;
  max-width: 380px;
  flex: 1;
}

.search-box input {
  width: 100%;
  height: 42px;
  border: 1px solid #d0d5dd;
  border-radius: 10px;
  padding: 0 14px;
  font-size: 14px;
  background: #fff;
  outline: none;
  box-sizing: border-box;
}

.search-box input:focus {
  border-color: #2f80ed;
  box-shadow: 0 0 0 3px rgba(47, 128, 237, 0.12);
}

.toolbar-actions {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.toolbar-actions button {
  height: 38px;
  padding: 0 14px;
  border: 1px solid #d0d5dd;
  background: #ffffff;
  border-radius: 10px;
  cursor: pointer;
  font-size: 13px;
  color: #344054;
}

.toolbar-actions button:hover {
  background: #f9fafb;
}

.chart-container {
  width: 100%;
  height: calc(100vh - 150px);
  min-height: 650px;
  background: #fff;
  border: 1px solid #eaecf0;
  border-radius: 16px;
  overflow: hidden;
}
.svg-chart-container {
  width: 100%;
  height: 100%;
}
</style>
