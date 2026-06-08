import { http } from "boot/axios";

export default {
  getCountries () {
    return http.get("/common/countries").then(response => response.data);
  },

  getLeadStages () {
    return http.get("/common/lead-stages").then(response => response.data);
  },

  getLeadActivities () {
    return http.get("/common/lead-activities").then(response => response.data);
  },

  getStates (countryId) {
    return http.get(`/common/state-provinces?countryId=${countryId}`).then(response => response.data);
  },

  getDashboardData () {
    return http.get("/common/user-dashbord").then(response => response.data);
  },

  getState (Stateid) {
    return http.get(`/common/${Stateid}`).then(response => response.data);
  },

  getCountry (Countryid) {
    return http.get(`/common/countries/${Countryid}`).then(response => response.data);
  },

  getValidationDetailsByCountryId (Countryid) {
    return http.get(`/common/validation-details/${Countryid}`).then(response => response.data);
  },

  getDropDown (type) {
    return http.get(`/drop-downs-type/list?type=${type}`).then(response => response.data);
  },
  getDropdownTypeByGroupName (groupName) {
    return http.get(`/drop-downs-type/dropDownTypeByGroupName?groupName=${groupName}`).then(response => response.data);
  },
  getDropdownByTypeId (typeId) {
    return http.get(`/drop-downs-type/dropDownValueByTypeId?typeId=${typeId}`).then(response => response.data);
  },
  getDropDownForSite (siteId, type) {
    return http.get(`/drop-downs-type/dropdownlist?siteId=${siteId}&type=${type}`).then(response => response.data);
  },
  getDropDownValue (id) {
    return http.get(`/drop-downs/${id}`).then(response => response.data);
  },
  getDropDownByTypeNameAndName (TypeName, Name) {
    return http.get(`/drop-downs/GetByTypeNameandName/${TypeName}/${Name}`).then(response => response.data);
  },
  getDropdownByButton (buttonType, type) {
    return http.get(`/drop-downs-type/dropdown-list/by-button?buttonType=${buttonType}&type=${type}`).then(response => response.data);
  },
  getAllNoteByTypeAndRecord (id, type, latestOnTop) {
    return http.get(`/notes/?subModuleId=${id}&type=${type}&latestOnTop=${latestOnTop}`).then(response => response.data);
  },
  getEmployeeIdBySiteId (siteId) {
    return http.get(`/common/employeeId/?siteId=${siteId}`).then(response => response.data);
  },
  saveNote (model) {
    return http.post("/notes", model).then(response => response.data);
  },
  deleteNote (id) {
    return http.delete(`/notes/deletenote/?id=${id}`).then(response => response.data);
  },
  getUser (userid) {
    return http.get(`/auth/user/?userid=${userid}`).then(response => response.data);
  },
  saveFiles (model) {
    return http.post("/common/saveFiles", model, { headers: { "Content-Type": "multipart/form-data" } }).then(response => response.data);
  },

  filterRowsBySearchTerm (data, searchTerm, columns) {
    if (!searchTerm) return data; // If no filter, return all data
    const lowerCaseTerm = searchTerm.toLowerCase();

    return data.filter(row =>
      columns.some(column => {
        let value;
        // Check if column.field is a function or a string
        if (typeof column.field === "function") {
          value = column.field(row); // Call the function
        } else if (typeof column.field === "string") {
          value = column.field.split(".").reduce((obj, key) => obj?.[key], row); // Handle nested fields
        }

        return String(value || "").toLowerCase().includes(lowerCaseTerm);
      })
    );
  }
};
