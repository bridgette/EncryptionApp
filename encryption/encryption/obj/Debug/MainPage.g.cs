﻿

#pragma checksum "C:\Users\breiche\Desktop\EncryptionApp\encryption\encryption\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "C88CF0B578CC3AE5CEF31917B8A7F4CA"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace encryption
{
    partial class MainPage : global::Windows.UI.Xaml.Controls.Page, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 16 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Tapped += this.newmessage_ontap;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 21 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Tapped += this.getkey_ontap;
                 #line default
                 #line hidden
                break;
            case 3:
                #line 22 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Tapped += this.createkey_ontap;
                 #line default
                 #line hidden
                break;
            case 4:
                #line 23 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Tapped += this.sharekey_ontap;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}


