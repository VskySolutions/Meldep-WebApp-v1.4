using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MimeKit;
using MimeKit.Text;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using Vsky.Core.Infrastructure;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.Messages
{
    public partial class EmailSender : IEmailSender
    {
        #region Fields

        private readonly IAppFileProvider _fileProvider;
        private readonly ISmtpBuilder _smtpBuilder;
        private readonly ApplicationDbContext _db;

        #endregion

        #region Ctor

        public EmailSender(IAppFileProvider fileProvider, 
                           ISmtpBuilder smtpBuilder,
                           ApplicationDbContext db)
        {
            _fileProvider = fileProvider;
            _smtpBuilder = smtpBuilder;
            _db = db;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Create an file attachment for the specific file path
        /// </summary>
        /// <param name="filePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains a leaf-node MIME part that contains an attachment.
        /// </returns>
        private async Task<MimePart> CreateMimeAttachmentAsync(string filePath, string attachmentFileName = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (string.IsNullOrWhiteSpace(attachmentFileName))
            {
                attachmentFileName = Path.GetFileName(filePath);
            }

            return CreateMimeAttachment(attachmentFileName, await _fileProvider.ReadAllBytesAsync(filePath), _fileProvider.GetCreationTime(filePath),
                _fileProvider.GetLastWriteTime(filePath), _fileProvider.GetLastAccessTime(filePath));
        }

        /// <summary>
        /// Create an file attachment for the binary data
        /// </summary>
        /// <param name="attachmentFileName">Attachment file name</param>
        /// <param name="binaryContent">The array of unsigned bytes from which to create the attachment stream.</param>
        /// <param name="cDate">Creation date and time for the specified file or directory</param>
        /// <param name="mDate">Date and time that the specified file or directory was last written to</param>
        /// <param name="rDate">Date and time that the specified file or directory was last access to.</param>
        /// <returns>A leaf-node MIME part that contains an attachment.</returns>
        private static MimePart CreateMimeAttachment(string attachmentFileName, byte[] binaryContent, DateTime cDate, DateTime mDate, DateTime rDate)
        {
            if (!ContentType.TryParse(MimeTypes.GetMimeType(attachmentFileName), out var mimeContentType))
            {
                mimeContentType = new ContentType("application", "octet-stream");
            }

            return new MimePart(mimeContentType)
            {
                FileName = attachmentFileName,
                Content = new MimeContent(new MemoryStream(binaryContent)),
                ContentDisposition = new ContentDisposition
                {
                    CreationDate = cDate,
                    ModificationDate = mDate,
                    ReadDate = rDate
                },
                ContentTransferEncoding = ContentEncoding.Base64
            };
        }

        #endregion

        #region Methods
        /// <summary>
        /// Sends an email
        /// </summary>
        /// <param name="emailAccount">Email account to use</param>
        /// <param name="subject">Subject</param>
        /// <param name="body">Body</param>
        /// <param name="fromAddress">From address</param>
        /// <param name="fromName">From display name</param>
        /// <param name="toAddress">To address</param>
        /// <param name="toName">To display name</param>
        /// <param name="replyTo">ReplyTo address</param>
        /// <param name="replyToName">ReplyTo display name</param>
        /// <param name="bcc">BCC addresses list</param>
        /// <param name="cc">CC addresses list</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
        /// <param name="attachedDownloadId">Attachment download ID (another attachment)</param>
        /// <param name="headers">Headers</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task SendEmailAsync(EmailAccount emailAccount, string subject, string body,
            string fromAddress, string fromName, string toAddress, string toName,
            string replyTo = null, string replyToName = null,
            IEnumerable<string> bcc = null, IEnumerable<string> cc = null,
            string attachmentFilePath = null, string attachmentFileName = null,
            IDictionary<string, string> headers = null)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(fromName, fromAddress));
            //message.To.Add(new MailboxAddress(toName, toAddress));

            // -----------------------------
            // TO (comma separated support)
            // -----------------------------
            if (!string.IsNullOrWhiteSpace(toAddress))
            {
                var toEmails = toAddress
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Distinct();

                foreach (var email in toEmails)
                {
                    message.To.Add(new MailboxAddress(toName, email));
                }
            }

            if (!string.IsNullOrEmpty(replyTo))
            {
                message.ReplyTo.Add(new MailboxAddress(replyToName, replyTo));
            }

            // bcc
            if (bcc != null)
            {
                foreach (var address in bcc.Where(bccValue => !string.IsNullOrWhiteSpace(bccValue)))
                {
                    message.Bcc.Add(new MailboxAddress("", address.Trim()));
                }
            }

            // cc
            if (cc != null)
            {
                foreach (var address in cc.Where(ccValue => !string.IsNullOrWhiteSpace(ccValue)))
                {
                    message.Cc.Add(new MailboxAddress("", address.Trim()));
                }
            }

            // content
            message.Subject = subject;

            // headers
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    message.Headers.Add(header.Key, header.Value);
                }
            }

            var multipart = new Multipart("mixed")
            {
                new TextPart(TextFormat.Html) { Text = body }
            };

            // create the file attachment for this e-mail message
            if (!string.IsNullOrEmpty(attachmentFilePath) && _fileProvider.FileExists(attachmentFilePath))
            {
                multipart.Add(await CreateMimeAttachmentAsync(attachmentFilePath, attachmentFileName));
            }

            message.Body = multipart;

            // send email
            using (var smtpClient = await _smtpBuilder.BuildAsync(emailAccount))
            {
                await smtpClient.SendAsync(message);

                await smtpClient.DisconnectAsync(true);
            }
        }
        #endregion

        #region Twilio Send EmailsAsync
        /// <summary>
        /// Twilio Send EmailsAsync
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="fromName"></param>
        /// <param name="toAddress"></param>
        /// <param name="toName"></param>
        /// <param name="replyTo"></param>
        /// <param name="replyToName"></param>
        /// <param name="bcc"></param>
        /// <param name="cc"></param>
        /// <param name="attachmentFilePath"></param>
        /// <param name="attachmentFileName"></param>
        /// <param name="headers"></param>
        /// <param name="token"></param>
        /// <param name="trackingId"></param>
        /// <returns></returns>
        public async Task<string> TwilioSendEmailsAsync(string subject,
                                                              string body,
                                                              string toAddress,
                                                              string toName,
                                                              string redirectionUrl,
                                                              string replyTo = null,
                                                              string replyToName = null,
                                                              IEnumerable<string> bcc = null,
                                                              IEnumerable<string> cc = null,
                                                              string attachmentFilePath = null,
                                                              string attachmentFileName = null,
                                                              IDictionary<string, string> headers = null,
                                                              string token = null,
                                                              string siteId = null)
        {
            try
            {
                // Load Twilio SendGrid settings from DB
                var emailSetting = _db?.TwilioEmailSettings?.FirstOrDefault(x => x.Active && !x.Deleted && x.SiteId == siteId);

                if (emailSetting == null || string.IsNullOrWhiteSpace(emailSetting.SendGridApiKey))
                {
                    throw new Exception("SendGrid settings not configured.");
                }

                //string fromAddress = emailSetting.FromEmail;
                string fromAddress = emailSetting.FromEmail;
                string replyAddress = $"supportteam+{token}@{redirectionUrl}";

                // Initialize SendGrid client
                var sendGridClient = new SendGridClient(emailSetting.SendGridApiKey);

                // Generate plain-text version from HTML
                var plainTextContent = string.IsNullOrEmpty(body) ? "" : Regex.Replace(body, "<.*?>", string.Empty);

                // Build email message
                var from = new EmailAddress(fromAddress ?? "no-reply@meldep.com", "MeldEp System Emails");
                //var to = new EmailAddress(toAddress, toName ?? "");

                // -----------------------------
                // Create message with multiple TOs
                // -----------------------------
                var msg = new SendGridMessage
                {
                    From = from,
                    Subject = subject ?? "",
                    HtmlContent = body ?? "",
                    PlainTextContent = plainTextContent
                };

                // TO addresses (comma-separated)
                if (!string.IsNullOrWhiteSpace(toAddress))
                {
                    var toEmails = toAddress
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Distinct();

                    foreach (var email in toEmails)
                        msg.AddTo(new EmailAddress(email, toName ?? ""));
                }              

                //var msg = MailHelper.CreateSingleEmail(from, to, subject ?? "", plainTextContent, body ?? "");

                //var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, body);

                //msg.AddCustomArgs(new Dictionary<string, string>
                //{
                //    { "db_request_id", trackingId }
                //});

                msg.HtmlContent = body;

                // set ReplyTo:
                if (!string.IsNullOrEmpty(replyAddress))
                    msg.ReplyTo = new EmailAddress(replyAddress, "MeldEp System Team");

                // Add CCs
                if (cc != null)
                {
                    foreach (var ccAddr in cc.Where(x => !string.IsNullOrWhiteSpace(x)))
                        msg.AddCc(ccAddr.Trim());
                }

                // Add BCCs
                if (bcc != null)
                {
                    foreach (var bccAddr in bcc.Where(x => !string.IsNullOrWhiteSpace(x)))
                        msg.AddBcc(bccAddr.Trim());
                }

                // Add Headers
                if (headers != null)
                {
                    foreach (var header in headers)
                        msg.AddHeader(header.Key, header.Value);
                }

                // Attachments
                if (!string.IsNullOrEmpty(attachmentFilePath))
                {
                    try
                    {
                        var fileBytes = await ReadFileBytesAsync(attachmentFilePath);
                        var filename = string.IsNullOrEmpty(attachmentFileName)
                                       ? Path.GetFileName(attachmentFilePath)
                                       : attachmentFileName;

                        var base64Content = Convert.ToBase64String(fileBytes);
                        msg.AddAttachment(filename, base64Content);
                    }
                    catch (FileNotFoundException)
                    {
                        // log or ignore
                    }
                }

                // Send the email
                var response = await sendGridClient.SendEmailAsync(msg).ConfigureAwait(false);

                // SendGrid success codes: 200 OK, 202 Accepted
                if (response != null &&
                    (response.StatusCode == System.Net.HttpStatusCode.OK ||
                     response.StatusCode == System.Net.HttpStatusCode.Accepted))
                {
                    return "sent"; // Success - return empty string
                }
                if (response?.Body != null)
                {
                    var content = await response.Body.ReadAsStringAsync();
                    var jsonResponse = JsonConvert.DeserializeObject<dynamic>(content);

                    if (jsonResponse?.errors != null)
                    {
                        foreach (var error in jsonResponse.errors)
                        {
                            var message = error.message?.ToString();
                            if (!string.IsNullOrEmpty(message))
                                return message;
                        }
                    }
                }
                return $"SendGrid API error: {response?.StatusCode}";
            }
            catch (Exception ex)
            {
                // TODO: log exception for debugging
                return $"Exception occurred while sending email: {ex.Message}";
            }
        }

        // Helper used by the method above.
        private async Task<byte[]> ReadFileBytesAsync(string path)
        {
            if (_fileProvider != null)
            {
                try
                {
                    var fileInfo = _fileProvider.GetFileInfo(path);
                    if (fileInfo != null && fileInfo.Exists)
                    {
                        using var stream = fileInfo.CreateReadStream();
                        using var ms = new MemoryStream();
                        await stream.CopyToAsync(ms);
                        return ms.ToArray();
                    }
                }
                catch
                {
                    // ignore and fallback
                }
            }

            if (File.Exists(path))
                return await File.ReadAllBytesAsync(path);

            throw new FileNotFoundException($"Attachment not found: {path}");
        }
        #endregion
    }
}