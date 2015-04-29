using encryption.Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace encryption
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;

        private FileActivatedEventArgs _fileEventArgs = null;
        public FileActivatedEventArgs FileEvent
        {
            get { return _fileEventArgs; }
            set { _fileEventArgs = value; }
        }

        public MainPage()
        {
            Current = this;  
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            KeyStore.Instance.Init();
            //PopulateContacts.CreateContactStore();

        }

        private static string FIRST_RUN_LOOKUP = "FIRST_RUN";
        public bool IsFirstRun()
        {            
            var localsettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var has_been_run = localsettings.Values[FIRST_RUN_LOOKUP];
            if (has_been_run == null)
            {
                localsettings.Values[FIRST_RUN_LOOKUP] = "42";
                return true;
            }
            else
            {
                return false;
            }
        }

        public void NavigateToFilePage()
        {
            if (FileEvent.Files != null && FileEvent.Files.Count > 0)
            {
                // Go process key file
                if (FileEvent.Files[0].Name.ToLower().EndsWith(".pgpkey"))
                {
                    this.Frame.Navigate(typeof(GetKeyPage), FileEvent.Files[0]);
                }
                // go process message
                else if (FileEvent.Files[0].Name.ToLower().EndsWith(".pgp"))
                {
                    this.Frame.Navigate(typeof(ReadMessagePage), FileEvent.Files[0]);
                }
            }
        }


        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private void sharekey_ontap(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ShareKeyPage));
        }

        private void getkey_ontap(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(GetKeyPage));
        }

        private void newmessage_ontap(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(NewMessagePage));
        }

        bool ranOnceThisSession = false;
        private void mainpage_hasframe(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            bool isFirstRun = IsFirstRun();
            if (isFirstRun && !ranOnceThisSession)
            {
                this.Frame.Navigate(typeof(GenerateKeyPage));
                ranOnceThisSession = true;
            }
        }
    }
}
