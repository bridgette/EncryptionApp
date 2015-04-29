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
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace encryption
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NewMessagePage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public NewMessagePage()
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
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }
        #endregion

        /// <summary>
        /// erase prepopulated text, change color from grey to black
        /// save watermark text for later.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        string inital_watermark_text = "Enter message here";
        private void messagetextbox_gotfocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            TextBox t = sender as TextBox;
            if (t.Text == inital_watermark_text)
            {
                t.Text = string.Empty;
                t.Foreground = new SolidColorBrush(Windows.UI.Colors.Black);
            }
        }

        /// <summary>
        /// if user went away without entering anything, autofill with prepopulated text
        // and switch back to grey
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void messagetextbox_lostfocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            TextBox t = sender as TextBox;
            if (t.Text.Equals(string.Empty))
            {
                t.Text = inital_watermark_text;
                t.Foreground = new SolidColorBrush(Windows.UI.Colors.LightGray);
            }
        }

        Contact contact;
        byte[] publicKey;

        private async void pickcontact_tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {

            var contactPicker = new Windows.ApplicationModel.Contacts.ContactPicker();
            contactPicker.DesiredFieldsWithContactFieldType.Add(ContactFieldType.Email);
            contact = await contactPicker.PickContactAsync();

            if (contact != null)
            {
                string email = contact.Emails.FirstOrDefault().Address;
                explaination_textblock.Text = contact.DisplayName;

                if (email != null)
                {
                    publicKey = await KeyStore.Instance.GetPublicKey(email);
                }

                if (publicKey != null)
                {
                    key_found_textblock.Text = "Key for contact found";
                }
                else
                {                    
                    key_found_textblock.Text = "Contact has no key :-(";
                }
            }

            return;
        }

        /// <summary>
        /// </summary>
        TextBlock key_found_textblock;
        private void grid_loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            key_found_textblock = ((Grid)sender).Children[2] as TextBlock;
        }

        private async void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            byte[] message = Encoding.UTF8.GetBytes(message_textbox.Text);

            if (publicKey != null)
            {
                byte[] dataToSend = PGP.Encrypt(message, publicKey, true, true);
               
                await MailHelper.ComposeEmailAsync(contact, "Encoded message", string.Empty, "message.pgp", dataToSend);                
            }
        }


    }
}