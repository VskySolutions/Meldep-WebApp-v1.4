// RequirementExportAction (WO-74): exports the clicked Requirement List row through
// GET /requirement/{id}/export (WO-73 contract). Framework-free so it can be unit tested with
// `node --test`; the page injects the service call, the download helper and the notify helpers.

export const XLSX_MIME = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

const ERROR_PREFIX = "Couldn't export the requirement.";

const escapeHtml = (text) => String(text).replace(/[&<>"']/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", "\"": "&quot;", "'": "&#39;" }[c]));

// ---------------------------------------------------------------------------------------------
// File names
// ---------------------------------------------------------------------------------------------

// Strips path parts, characters Windows/macOS reject and control characters, and forces .xlsx.
export function sanitizeExportFilename (name) {
  if (typeof name !== "string") return null;
  let value = name.split(/[\\/]/).pop() ?? "";
  // eslint-disable-next-line no-control-regex
  value = value.replace(/[\u0000-\u001f\u007f<>:"|?*]/g, "_").replace(/^[.\s]+|[.\s]+$/g, "").trim();
  if (!value) return null;
  if (!/\.xlsx$/i.test(value)) value = `${value.replace(/\.[^.]*$/, "")}.xlsx`;
  return value.length > 150 ? `${value.slice(0, 145)}.xlsx` : value;
}

export function fallbackExportFilename (row) {
  const identity = row?.requirementNumber ? String(row.requirementNumber) : String(row?.id ?? "export");
  return sanitizeExportFilename(`Requirement-${identity.replace(/[^A-Za-z0-9_-]/g, "_")}.xlsx`);
}

// Reads RFC 6266 filename* (UTF-8) first, then filename. Returns null when absent: the API's CORS
// setup does not currently expose Content-Disposition, so the fallback is the normal path.
export function filenameFromContentDisposition (header) {
  if (!header || typeof header !== "string") return null;

  const star = /filename\*\s*=\s*(?:UTF-8|utf-8)''([^;]+)/.exec(header);
  if (star) {
    try {
      return sanitizeExportFilename(decodeURIComponent(star[1].trim().replace(/^"|"$/g, "")));
    } catch {
      // malformed percent-encoding: fall through to the plain parameter
    }
  }

  const plain = /filename\s*=\s*("([^"]*)"|[^;]+)/i.exec(header);
  if (!plain) return null;
  return sanitizeExportFilename((plain[2] ?? plain[1]).trim());
}

function headerValue (headers, name) {
  if (!headers) return null;
  if (typeof headers.get === "function") return headers.get(name) ?? null;
  const key = Object.keys(headers).find(k => k.toLowerCase() === name.toLowerCase());
  return key ? headers[key] : null;
}

// ---------------------------------------------------------------------------------------------
// Response checks
// ---------------------------------------------------------------------------------------------

async function readBlobText (blob) {
  if (blob == null) return "";
  if (typeof blob === "string") return blob;
  if (typeof blob.text === "function") return blob.text();
  return "";
}

async function startsWithZipSignature (blob) {
  if (!blob || typeof blob.slice !== "function") return false;
  const head = blob.slice(0, 2);
  const buffer = typeof head.arrayBuffer === "function" ? await head.arrayBuffer() : null;
  if (!buffer) return false;
  const bytes = new Uint8Array(buffer);
  return bytes.length === 2 && bytes[0] === 0x50 && bytes[1] === 0x4b; // "PK": .xlsx is a zip package
}

// A 200 is only a workbook when it looks like one; an error document must never be saved as .xlsx.
export async function assertWorkbookResponse (response) {
  const blob = response?.data;
  const type = String(headerValue(response?.headers, "content-type") ?? blob?.type ?? "").toLowerCase();
  const looksTyped = !type || type.includes("spreadsheetml") || type.includes("octet-stream");

  if (!blob || !looksTyped || !(await startsWithZipSignature(blob))) {
    const error = new Error("The server did not return an Excel workbook.");
    error.response = { status: response?.status ?? 200, data: blob, headers: response?.headers };
    error.invalidWorkbook = true;
    throw error;
  }
  return blob;
}

// Turns an API failure into an application-standard, HTML-escaped message (notify renders HTML).
// With responseType "blob", JSON error bodies arrive as Blobs and are decoded here.
export async function describeExportError (error) {
  if (error?.invalidWorkbook) return escapeHtml(`${ERROR_PREFIX} The server returned an unexpected response. Please try again.`);

  const response = error?.response;
  if (!response) return escapeHtml(`${ERROR_PREFIX} The server could not be reached. Check your connection and try again.`);

  let detail = "";
  try {
    const text = await readBlobText(response.data);
    try {
      const parsed = JSON.parse(text);
      detail = typeof parsed === "string" ? parsed : (parsed?.message || parsed?.title || "");
    } catch {
      detail = /<\s*html/i.test(text) ? "" : text;
    }
  } catch {
    detail = "";
  }
  detail = String(detail || "").trim().slice(0, 300);

  switch (response.status) {
    case 401:
      return escapeHtml(`${ERROR_PREFIX} Your session has expired. Please sign in again.`);
    case 403:
      return escapeHtml(`${ERROR_PREFIX} ${detail || "You don't have permission to export this requirement."}`);
    case 404:
      return escapeHtml(`${ERROR_PREFIX} The requirement was not found, or requirement export isn't enabled yet.`);
    default:
      return escapeHtml(detail ? `${ERROR_PREFIX} ${detail}` : `${ERROR_PREFIX} Please try again. (HTTP ${response.status})`);
  }
}

// ---------------------------------------------------------------------------------------------
// Row action
// ---------------------------------------------------------------------------------------------

/**
 * @param {object} deps
 * @param {(id: string) => Promise<{data: Blob, headers: object, status: number}>} deps.exportRequirement
 * @param {(blob: Blob, filename: string) => void} deps.download
 * @param {(n: {message: string}) => void} deps.notifyError
 * @param {(n: {message: string}) => void} [deps.notifySuccess]
 * @param {object} [deps.pending] map of requirementId -> true; pass reactive({}) from Vue
 */
export function createRequirementExportAction ({ exportRequirement, download, notifyError, notifySuccess, pending = {} }) {
  const isExporting = (id) => Boolean(id && pending[id]);

  // Exports exactly the given row. Bulk selection, filters and paging are never read.
  async function exportRow (row) {
    const id = row?.id;
    if (!id || isExporting(id)) return false;

    pending[id] = true;
    try {
      const response = await exportRequirement(id);
      const blob = await assertWorkbookResponse(response);
      const filename = filenameFromContentDisposition(headerValue(response.headers, "content-disposition")) ?? fallbackExportFilename(row);
      download(blob, filename);
      notifySuccess?.({ message: escapeHtml(row.requirementNumber ? `Requirement #${row.requirementNumber} exported.` : "Requirement exported.") });
      return true;
    } catch (error) {
      notifyError({ message: await describeExportError(error) });
      return false;
    } finally {
      delete pending[id];
    }
  }

  return { pending, isExporting, exportRow };
}
