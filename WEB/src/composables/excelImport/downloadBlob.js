// Saves a Blob (e.g. a backend-generated .xlsx template) to the user's device.
export function downloadBlob (blob, filename) {
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}

const escapeHtml = (text) => String(text).replace(/[&<>"']/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", "\"": "&quot;", "'": "&#39;" }[c]));

// Builds a user-facing reason for a failed blob download. With responseType "blob" the server's error body
// arrives as a Blob, so it is read back as text here. The result is HTML-escaped because the app's notify
// helpers render HTML.
export async function describeDownloadError (error, fallback) {
  const status = error?.response?.status;
  if (!error?.response) return escapeHtml(`${fallback} The server could not be reached.`);
  if (status === 404) return escapeHtml(`${fallback} The server does not provide this template yet; please restart or redeploy the API.`);
  if (status === 401 || status === 403) return escapeHtml(`${fallback} You are not authorized; please sign in again.`);

  let detail = "";
  const data = error.response.data;
  try {
    const text = data instanceof Blob ? await data.text() : (typeof data === "string" ? data : JSON.stringify(data ?? ""));
    try {
      const parsed = JSON.parse(text);
      detail = typeof parsed === "string" ? parsed : (parsed?.message || parsed?.title || "");
    } catch {
      detail = text;
    }
  } catch {
    detail = "";
  }
  detail = String(detail || "").trim().slice(0, 300);
  return escapeHtml(detail ? `${fallback} ${detail}` : `${fallback} (HTTP ${status})`);
}
