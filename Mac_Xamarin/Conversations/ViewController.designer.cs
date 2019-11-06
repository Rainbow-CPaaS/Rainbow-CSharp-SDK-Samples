// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace SampleConversation
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSBox box { get; set; }

		[Outlet]
		AppKit.NSButton btnContact_AddConv { get; set; }

		[Outlet]
		AppKit.NSButton btnContacts_AddToFav { get; set; }

		[Outlet]
		AppKit.NSButton btnConversations_AddToFav { get; set; }

		[Outlet]
		AppKit.NSButton btnConversations_Remove { get; set; }

		[Outlet]
		AppKit.NSButton btnLogin { get; set; }

		[Outlet]
		AppKit.NSButton btnRemoveFav_Clicked { get; set; }

		[Outlet]
		AppKit.NSPopUpButton cbContacts { get; set; }

		[Outlet]
		AppKit.NSPopUpButton cbConversations { get; set; }

		[Outlet]
		AppKit.NSPopUpButton cbFavorites { get; set; }

		[Outlet]
		AppKit.NSTextField txt_login { get; set; }

		[Outlet]
		AppKit.NSTextField txt_password { get; set; }

		[Outlet]
		AppKit.NSTextView txt_state { get; set; }

		[Outlet]
		AppKit.NSTextField txtPosition { get; set; }

		[Action ("btnAddConversation_Clicked:")]
		partial void btnAddConversation_Clicked (Foundation.NSObject sender);

		[Action ("btnContacts_AddToFav_Clicked:")]
		partial void btnContacts_AddToFav_Clicked (Foundation.NSObject sender);

		[Action ("btnConversations_AddToFav_Clicked:")]
		partial void btnConversations_AddToFav_Clicked (Foundation.NSObject sender);

		[Action ("btnFavoriteRemove_Clicked:")]
		partial void btnFavoriteRemove_Clicked (Foundation.NSObject sender);

		[Action ("btnLoginClicked:")]
		partial void btnLoginClicked (Foundation.NSObject sender);

		[Action ("btnRemoveConversations_Clicked:")]
		partial void btnRemoveConversations_Clicked (Foundation.NSObject sender);

		[Action ("btnUpdateFavPosition_Clicked:")]
		partial void btnUpdateFavPosition_Clicked (Foundation.NSObject sender);

		[Action ("cbContacts_SelectionChanged:")]
		partial void cbContacts_SelectionChanged (Foundation.NSObject sender);

		[Action ("cbConversations_SelectionChanged:")]
		partial void cbConversations_SelectionChanged (Foundation.NSObject sender);

		[Action ("cbFavorites_SelectionChanged:")]
		partial void cbFavorites_SelectionChanged (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (box != null) {
				box.Dispose ();
				box = null;
			}

			if (btnConversations_Remove != null) {
				btnConversations_Remove.Dispose ();
				btnConversations_Remove = null;
			}

			if (btnContacts_AddToFav != null) {
				btnContacts_AddToFav.Dispose ();
				btnContacts_AddToFav = null;
			}

			if (btnContact_AddConv != null) {
				btnContact_AddConv.Dispose ();
				btnContact_AddConv = null;
			}

			if (btnConversations_AddToFav != null) {
				btnConversations_AddToFav.Dispose ();
				btnConversations_AddToFav = null;
			}

			if (btnLogin != null) {
				btnLogin.Dispose ();
				btnLogin = null;
			}

			if (btnRemoveFav_Clicked != null) {
				btnRemoveFav_Clicked.Dispose ();
				btnRemoveFav_Clicked = null;
			}

			if (cbContacts != null) {
				cbContacts.Dispose ();
				cbContacts = null;
			}

			if (cbConversations != null) {
				cbConversations.Dispose ();
				cbConversations = null;
			}

			if (cbFavorites != null) {
				cbFavorites.Dispose ();
				cbFavorites = null;
			}

			if (txt_login != null) {
				txt_login.Dispose ();
				txt_login = null;
			}

			if (txt_password != null) {
				txt_password.Dispose ();
				txt_password = null;
			}

			if (txt_state != null) {
				txt_state.Dispose ();
				txt_state = null;
			}

			if (txtPosition != null) {
				txtPosition.Dispose ();
				txtPosition = null;
			}
		}
	}
}
