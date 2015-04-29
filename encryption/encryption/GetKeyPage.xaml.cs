using encryption.Common;
using encryption.Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Contacts;
using Org.BouncyCastle.Bcpg;
using Windows.Storage;
using Windows.Storage.Streams;


// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace encryption
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GetKeyPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        IStorageItem PublicKeyFile;

        public GetKeyPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            PublicKeyFile = (e.Parameter as IStorageItem);
            if (PublicKeyFile != null)
                filename_box.Text = PublicKeyFile.Name;

            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }
        #endregion

        string contact_email;
        private async void savekey_ontap(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {

            if (contact_email != null && PublicKeyFile != null)
            {           
                // Open the file
                StorageFile sampleFile = (PublicKeyFile as StorageFile); 
                
                var buffer = await FileIO.ReadBufferAsync(sampleFile);

                DataReader dataReader = Windows.Storage.Streams.DataReader.FromBuffer(buffer);
                byte[] fileContent = new byte[dataReader.UnconsumedBufferLength]; 
                dataReader.ReadBytes(fileContent);                 
                
                await KeyStore.Instance.AddPublicKey(contact_email, fileContent );

                this.Frame.Navigate(typeof(MainPage));
            }
        }
       
        private async void selectcontact_ontap(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var contactPicker = new Windows.ApplicationModel.Contacts.ContactPicker();
            contactPicker.DesiredFieldsWithContactFieldType.Add(ContactFieldType.Email);
            Contact contact = await contactPicker.PickContactAsync();

            if (contact != null)
            {
                contact_email = contact.Emails.FirstOrDefault().Address;
                selectcontact_textblock.Text = contact.DisplayName;
            }

            return;

        }

        /// <summary>
        /// erase prepopulated text, change color from grey to black
        /// save watermark text for later.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        string inital_watermark_text;
        private void copykeytextbox_gotfocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            TextBox t = sender as TextBox;
            inital_watermark_text = t.Text;
            t.Text = string.Empty;
        }

        /// <summary>
        /// if user went away without entering anything, autofill with prepopulated text
        // and switch back to grey
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void copykeytextbox_lostfocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            TextBox t = sender as TextBox;
            if (t.Text.Equals(string.Empty))
            {
                t.Text = inital_watermark_text;

            }
        }       
    }
}
