using System;
using System.Collections.Generic;
using Windows.Phone.PersonalInformation;

namespace encryption
{
    /// <summary>
    /// Sets phone configuration.
    /// </summary>
    public static class PopulateContacts
    {
        /// <summary>
        /// Creates a contact store and add contacts.
        /// </summary>
        public static async void CreateContactStore()
        {
            ContactStore contactStore = await ContactStore.CreateOrOpenAsync(
                            ContactStoreSystemAccessMode.ReadWrite,
                            ContactStoreApplicationAccessMode.ReadOnly);

            foreach (SampleContact sampleContact in SampleContact.CreateSampleContacts())
            {
                StoredContact contact = new StoredContact(contactStore);
                IDictionary<string, object> props = await contact.GetPropertiesAsync();

                if (!string.IsNullOrEmpty(sampleContact.FirstName))
                {
                    props.Add(KnownContactProperties.GivenName, sampleContact.FirstName);
                }

                if (!string.IsNullOrEmpty(sampleContact.LastName))
                {
                    props.Add(KnownContactProperties.FamilyName, sampleContact.LastName);
                }

                if (!string.IsNullOrEmpty(sampleContact.HomeEmail))
                {
                    props.Add(KnownContactProperties.Email, sampleContact.HomeEmail);
                }

                if (!string.IsNullOrEmpty(sampleContact.WorkEmail))
                {
                    props.Add(KnownContactProperties.WorkEmail, sampleContact.WorkEmail);
                }

                if (!string.IsNullOrEmpty(sampleContact.HomePhone))
                {
                    props.Add(KnownContactProperties.Telephone, sampleContact.HomePhone);
                }

                if (!string.IsNullOrEmpty(sampleContact.WorkPhone))
                {
                    props.Add(KnownContactProperties.CompanyTelephone, sampleContact.WorkPhone);
                }

                if (!string.IsNullOrEmpty(sampleContact.MobilePhone))
                {
                    props.Add(KnownContactProperties.MobileTelephone, sampleContact.MobilePhone);
                }

                await contact.SaveAsync();
            }
        }
    }

    /// <summary>
    /// Sample contact.
    /// </summary>
    public class SampleContact
    {
        /// <summary>
        ///  Initializes a new instance of the SampleContact class.
        /// </summary>
        public SampleContact()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        public string Id { get; private set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string HomeEmail { get; set; }

        public string WorkEmail { get; set; }

        public string HomePhone { get; set; }

        public string WorkPhone { get; set; }

        public string MobilePhone { get; set; }

        public string DisplayName
        {
            get
            {
                return this.FirstName + " " + this.LastName;
            }
        }

        /// <summary>
        /// Creates list of sample contacts.
        /// </summary>
        /// <returns>List of sample contacts</returns>
        public static List<SampleContact> CreateSampleContacts()
        {
            List<SampleContact> contacts = new List<SampleContact>
            {
                new SampleContact()
                {
                    FirstName = "Bridgette",
                    LastName = "E",
                    HomeEmail = "breiche@microsoft.com",
                    WorkEmail = "bridgette@outlook.com",
                },
                new SampleContact()
                {
                    FirstName = "Pavel",
                    LastName = "Bansky",
                    HomeEmail = "pavel.bansky@microsoft.com",

                }
            };

            return contacts;
        }
    }


}
