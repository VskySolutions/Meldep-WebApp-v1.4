import { http } from "boot/axios";

export default {
  getDropDownValues (model) {
    return http.post("/drop-downs/list", model).then(response => response.data);
  },

  getDropDownValue (id) {
    return http.get(`/drop-downs/${id}`).then(response => response.data);
  },

  saveDropDown (id, model) {
    if (id) {
      return http.put(`/drop-downs/${id}`, model).then(response => response.data);
    } else {
      return http.post("/drop-downs", model).then(response => response.data);
    }
  },

  deleteDropDown (id) {
    return http.delete(`/drop-downs/${id}`).then(response => response.data);
  },

  // Drop Down Type
  getDropDownTypes (model) {
    return http.post("/drop-downs-type/list", model).then(response => response.data);
  },

  getDropDownType (id) {
    return http.get(`/drop-downs-type/${id}`).then(response => response.data);
  },

  saveDropDownType (id, model) {
    if (id) {
      return http.put(`/drop-downs-type/${id}`, model).then(response => response.data);
    } else {
      return http.post("/drop-downs-type", model).then(response => response.data);
    }
  },

  saveBulkDropDownTypes (id, model) {
    if (id) {
      return http.put(`drop-downs-type/save-bulk-dropDown-types/${id}`, model).then(response => response.data);
    } else {
      return http.post("drop-downs-type/save-bulk-dropDown-types", model).then(response => response.data);
    }
  },

  deleteDropDownType (id) {
    return http.delete(`/drop-downs-type/${id}`).then(response => response.data);
  },

  getAllDropDownTypeListForDropdown () {
    return http.get("/drop-downs-type/dropdown/list").then(response => response.data);
  },

  // Manage Dropdown
  getDropdownTypeByModuleName (moduleName) {
    return http.get(`/drop-downs-type/dropDownTypeByModuleName?moduleName=${moduleName}`).then(response => response.data);
  },
  getDropdownTypesByIdAndGroupName (id, groupName) {
    return http.get(`/drop-downs-type/${id}/${groupName}`).then(response => response.data);
  }
};
