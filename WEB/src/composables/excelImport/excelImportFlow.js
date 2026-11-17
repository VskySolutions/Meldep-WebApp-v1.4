// Shared Excel import flow:- upload -> backend preview -> explicit confirmation.
//
// Framework-free state machine used by components/excel-import/_excelImportDialog.vue (which passes a Vue
// `reactive` object as `state`) and configured per module (see modules/test-plan/utils/importFlow.js and
// modules/test-case/utils/importFlow.js). Plain JS so it can be unit-tested with mocked service responses.
//
// The backend owns parsing, validation, duplicate checks and persistence. The client only:
//   - mirrors the API's file policy as a convenience (the API enforces it again),
//   - blocks confirmation unless a nonempty preview is entirely valid,
//   - ignores stale upload responses and invalidates the preview when the file is replaced,
//   - prevents repeat confirmation while one is pending,
//   - submits only the allowlisted canonical fields, never validity flags or server-owned metadata.
//
// config = {
//   fields:   [{ key, label, labelKey?, required }]   editable fields in template order
//   policy:   { extension, accept, maxFileSizeBytes, maxRows }
//   messages: { previewFailed, importFailed, fileType, success(data) -> string }
// }

export function createImportState () {
  return {
    file: null,
    phase: "idle", // idle | uploading | previewed | confirming | imported
    preview: null, // { rows, totalRows, validRows, invalidRows, canConfirm, raw }
    fileErrors: [], // [{ code, message }]
    resultMessage: "",
    resultType: "", // "" | "positive" | "negative"
    importedCount: 0,
    result: null // last successful confirmation response
  };
}

// Only allowlisted canonical fields go back to the API; isValid/errors/labels are dropped.
export function buildConfirmRows (rows, fields) {
  const keys = ["rowNumber", ...fields.map(f => f.key)];
  return (rows || []).map(row => {
    const out = {};
    for (const key of keys) out[key] = row[key] ?? null;
    return out;
  });
}

export function rowHasErrors (row) {
  return Array.isArray(row?.errors) && row.errors.length > 0;
}

export function fieldErrors (row, fieldKey) {
  return (row?.errors || []).filter(e => e.field === fieldKey);
}

// Errors not tied to one of the configured fields (e.g. row number problems).
export function rowLevelErrors (row, fields) {
  const keys = fields.map(f => f.key);
  return (row?.errors || []).filter(e => !e.field || !keys.includes(e.field));
}

export function canConfirm (state) {
  const p = state.preview;
  return state.phase === "previewed" &&
    state.fileErrors.length === 0 &&
    !!p && p.canConfirm === true &&
    Array.isArray(p.rows) && p.rows.length > 0 &&
    !p.rows.some(rowHasErrors);
}

export function checkFile (file, policy, messages = {}) {
  if (!file) return [{ code: "file_empty", message: "Please select an Excel file." }];
  const name = String(file.name || "").toLowerCase();
  if (!name.endsWith(policy.extension)) {
    return [{ code: "file_type", message: messages.fileType || "Only Excel .xlsx files created from the template are supported." }];
  }
  if (!file.size) return [{ code: "file_empty", message: "Please upload a non-empty Excel file." }];
  if (file.size > policy.maxFileSizeBytes) {
    return [{ code: "file_too_large", message: `The file exceeds the maximum size of ${Math.round(policy.maxFileSizeBytes / (1024 * 1024))} MB.` }];
  }
  return [];
}

function normalizePreview (data) {
  const rows = Array.isArray(data?.rows) ? data.rows : [];
  return {
    rows,
    totalRows: data?.totalRows ?? rows.length,
    validRows: data?.validRows ?? rows.filter(r => !rowHasErrors(r)).length,
    invalidRows: data?.invalidRows ?? rows.filter(rowHasErrors).length,
    canConfirm: data?.canConfirm === true,
    raw: data || {}
  };
}

// Extracts a plain-text API error message (string body, ApiError {message}, or result {message}).
export function errorMessage (error, fallback) {
  const body = error?.response?.data;
  if (typeof body === "string" && body.trim()) return body;
  if (body && typeof body.message === "string" && body.message.trim()) return body.message.split("~").join(" ");
  return fallback;
}

export function createExcelImportFlow (service, config, state = createImportState()) {
  const { fields, policy, messages } = config;
  let uploadSeq = 0;

  function clearPreview () {
    state.preview = null;
    state.fileErrors = [];
    state.resultMessage = "";
    state.resultType = "";
    state.importedCount = 0;
    state.result = null;
  }

  // Selecting/replacing a file always invalidates the previous preview; responses for older uploads are ignored.
  async function upload (file) {
    if (state.phase === "confirming") return { ignored: true };
    const seq = ++uploadSeq;
    clearPreview();
    state.file = file || null;

    if (!file) {
      state.phase = "idle";
      return { ignored: false };
    }

    const localErrors = checkFile(file, policy, messages);
    if (localErrors.length) {
      state.fileErrors = localErrors;
      state.phase = "idle";
      return { ignored: false };
    }

    state.phase = "uploading";
    try {
      const data = await service.previewImport(file);
      if (seq !== uploadSeq) return { ignored: true };
      const fileErrors = Array.isArray(data?.fileErrors) ? data.fileErrors : [];
      state.fileErrors = fileErrors;
      state.preview = fileErrors.length ? null : normalizePreview(data);
      state.phase = fileErrors.length ? "idle" : "previewed";
    } catch (error) {
      if (seq !== uploadSeq) return { ignored: true };
      state.preview = null;
      state.fileErrors = [{ code: "preview_failed", message: errorMessage(error, messages.previewFailed) }];
      state.phase = "idle";
    }
    return { ignored: false };
  }

  async function confirm () {
    if (!canConfirm(state)) return { success: false, skipped: true };

    state.phase = "confirming";
    state.resultMessage = "";
    state.resultType = "";
    const rows = buildConfirmRows(state.preview.rows, fields);

    try {
      const data = await service.confirmImport(rows);
      if (data?.success === true) {
        state.importedCount = Number(data.importedCount) || 0;
        state.result = data;
        state.resultMessage = messages.success({ ...data, importedCount: state.importedCount });
        state.resultType = "positive";
        state.phase = "imported";
        return { success: true, importedCount: state.importedCount, data };
      }
      return fail(data);
    } catch (error) {
      return fail(error?.response?.data, error);
    }
  }

  // Failure keeps reviewable results; confirmation-time validation (if returned) replaces the preview.
  function fail (body, error) {
    const validation = body && typeof body === "object" ? body.validation : null;
    if (validation) {
      state.preview = normalizePreview(validation);
      state.fileErrors = Array.isArray(validation.fileErrors) ? validation.fileErrors : [];
    }
    state.importedCount = 0;
    state.result = null;
    state.resultType = "negative";
    state.resultMessage = (body && typeof body === "object" && typeof body.message === "string" && body.message) ||
      errorMessage(error, messages.importFailed);
    state.phase = "previewed";
    return { success: false };
  }

  function reset () {
    uploadSeq++;
    clearPreview();
    state.file = null;
    state.phase = "idle";
  }

  return { state, config, upload, confirm, reset, canConfirm: () => canConfirm(state) };
}
