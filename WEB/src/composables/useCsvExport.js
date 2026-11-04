// Client-side CSV export of a list: the rows the reader can see, in the columns the reader can see.
//
//   exportCsv({ filename: "requirements.csv", columns: [{ label, value: (row) => text }], rows })
//
// Values are stringified and quoted; a leading =, +, - or @ is prefixed with an apostrophe so a cell can
// never execute as a formula when the file is opened in a spreadsheet.

const escapeCell = (value) => {
  let text = value == null ? "" : String(value);
  if (/^[=+\-@]/.test(text)) text = `'${text}`;
  if (/[",\r\n]/.test(text)) text = `"${text.replace(/"/g, "\"\"")}"`;
  return text;
};

export function buildCsv ({ columns, rows }) {
  const header = columns.map((c) => escapeCell(c.label)).join(",");
  const lines = rows.map((row) => columns.map((c) => escapeCell(typeof c.value === "function" ? c.value(row) : row[c.value])).join(","));
  return [header, ...lines].join("\r\n");
}

export function downloadText (text, filename, mime = "text/csv;charset=utf-8") {
  // The BOM makes Excel read UTF-8 accents correctly.
  const blob = new Blob(["﻿", text], { type: mime });
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}

export function exportCsv ({ filename, columns, rows }) {
  downloadText(buildCsv({ columns, rows }), filename);
}

export function useCsvExport () {
  return { exportCsv, buildCsv, downloadText };
}
