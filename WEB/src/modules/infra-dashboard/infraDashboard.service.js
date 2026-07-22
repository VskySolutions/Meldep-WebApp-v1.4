import { http } from "boot/axios";
import qs from "qs";

// Serializes filter params so list values become repeated query keys (providerIds=a&providerIds=b),
// which ASP.NET Core binds to the List<string> filter properties. Null/empty values are skipped.
// Tenant (X-Site-Id) and auth headers are injected automatically by the axios boot interceptor.
function requestConfig (params) {
  return {
    params,
    paramsSerializer: (p) => qs.stringify(p, { arrayFormat: "repeat", skipNulls: true })
  };
}

export default {
  getSummary (params) {
    return http.get("/infra-dashboard/summary", requestConfig(params)).then((r) => r.data);
  },
  getBreakdowns (params) {
    return http.get("/infra-dashboard/breakdowns", requestConfig(params)).then((r) => r.data);
  },
  getPriceChanges (params) {
    return http.get("/infra-dashboard/price-changes", requestConfig(params)).then((r) => r.data);
  },
  getHistory (params) {
    return http.get("/infra-dashboard/history", requestConfig(params)).then((r) => r.data);
  },
  getDataQuality (params) {
    return http.get("/infra-dashboard/data-quality", requestConfig(params)).then((r) => r.data);
  },
  getRenewals (params) {
    return http.get("/infra-dashboard/renewals", requestConfig(params)).then((r) => r.data);
  }
};
