<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important;max-width: 50vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">View Org Structure</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <!-- <q-card class="card-header with-tools headerBasic"> -->
          <fieldset>
            <legend>Org Structure Info</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Year</div>
                <div class="text-black">
                  {{ model.year ? model.year : "-" }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Level</div>
                <div class="text-black">
                  {{ model.level ? model.level : "-" }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Manager</div>
                <div class="text-black">
                  {{ model.manager.person.fullName ? model.manager.person.fullName : "-"  }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Employee</div>
                <div class="text-black">
                  {{ model.employee.person.fullName ? model.employee.person.fullName : "-" }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Department</div>
                <div class="text-black">
                  {{ model.department.name ? model.department.name : "-" }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Role</div>
                <div class="text-black">
                 <span>
                    {{
                      model.employeeOrgStructureDesignationMapping?.length
                        ? model.employeeOrgStructureDesignationMapping
                            .map(m => m.employeeDesignation.dropDownValue)
                            .join(', ')
                        : '-'
                    }}
                  </span>
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Sort Order</div>
                <div class="text-black">
                  {{ model.sortOrder ? model.sortOrder : "-" }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6 hidden">
                <div class="q-mb-xs">Color:</div>
                <div class="text-black">
                  {{ model.color ? model.color : "-" }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-12 col-md-12">
                <div class="q-mb-xs">Responsibilities:</div>
                <div class="text-black RichTextEditor">
                  <p v-html="model.responsibilities ? model.responsibilities : '-'" />
                </div>
              </div>
            </div>
          </fieldset>
        </div>
      </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { ref, onMounted } from "vue";
import _ from "lodash";
import orgStructureService from "modules/employee-org-structure/employeeOrgStructure.service";

// Common variables
// const { toDate } = useFilters();
const loading = ref(true);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Define model values
const model = ref({
  manager: {
    person:{
      fullName:""
    }
  },
  employee:{
    person:{
      fullName:""
    }
  },
  department:{
    name: ""
  },
});
// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// get job post details
const getEmployeeOrgStructure = () => {
  loading.value = true;
  orgStructureService.getEmployeeOrgStructure(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

// On page rendering
onMounted(() => {
  getEmployeeOrgStructure();
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
