using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace encryption.Common
{
    public class DeliveryManager
    {
        public async Task<bool> ComposeEmailAsync(Windows.ApplicationModel.Contacts.Contact recipient, string subject, string messageBody, string filename, byte[] fileContent)
        {
            var emailMessage = new Windows.ApplicationModel.Email.EmailMessage();
            emailMessage.Subject = subject;
            emailMessage.Body = messageBody;

            if (fileContent != null)
            {
                StorageFile newFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("tempfile", CreationCollisionOption.ReplaceExisting);
                await Windows.Storage.FileIO.WriteBytesAsync(newFile, fileContent);
                
                
                var attachment = new Windows.ApplicationModel.Email.EmailAttachment(
                    filename,
                    newFile);

                emailMessage.Attachments.Add(attachment);
            }

            if (recipient != null)
            {
                var email = recipient.Emails.FirstOrDefault<Windows.ApplicationModel.Contacts.ContactEmail>();
                if (email != null)
                {
                    var emailRecipient = new Windows.ApplicationModel.Email.EmailRecipient(email.Address);
                    emailMessage.To.Add(emailRecipient);
                }
            }

            await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMessage);

            return true;
        }
    }
}
