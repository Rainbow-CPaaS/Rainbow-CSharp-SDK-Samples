// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace SampleInstantMessaging
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSBox box { get; set; }

		[Outlet]
		AppKit.NSBox boxMessages { get; set; }

		[Outlet]
		AppKit.NSButton btnIsTyping { get; set; }

		[Outlet]
		AppKit.NSButton btnLogin { get; set; }

		[Outlet]
		AppKit.NSPopUpButton cbContacts { get; set; }

		[Outlet]
		AppKit.NSPopUpButton cbConversations { get; set; }

		[Outlet]
		AppKit.NSPopUpButton cbPresence { get; set; }

		[Outlet]
		AppKit.NSTextField txt_login { get; set; }

		[Outlet]
		AppKit.NSSecureTextField txt_password { get; set; }

		[Outlet]
		AppKit.NSTextView txt_state { get; set; }

		[Outlet]
		AppKit.NSTextField txtContactPresence { get; set; }

		[Outlet]
		AppKit.NSTextView txtMessagesExchanged { get; set; }

		[Outlet]
		AppKit.NSTextField txtMessageToSend { get; set; }

		[Outlet]
		AppKit.NSTextField txtSelectionInfo { get; set; }

		[Action ("btnIsTyping_CheckedChanged:")]
		partial void btnIsTyping_CheckedChanged (Foundation.NSObject sender);

		[Action ("btnLoadMessages_Clicked:")]
		partial void btnLoadMessages_Clicked (Foundation.NSObject sender);

		[Action ("btnLogin_Clicked:")]
		partial void btnLogin_Clicked (Foundation.NSObject sender);

		[Action ("btnMarkLastMessage_Clicked:")]
		partial void btnMarkLastMessage_Clicked (Foundation.NSObject sender);

		[Action ("btnSendMessage_Clicked:")]
		partial void btnSendMessage_Clicked (Foundation.NSObject sender);

		[Action ("cbContacts_SelectionChanged:")]
		partial void cbContacts_SelectionChanged (Foundation.NSObject sender);

		[Action ("cbConversations_SelectionChanged:")]
		partial void cbConversations_SelectionChanged (Foundation.NSObject sender);

		[Action ("cbPresence_SelectionChanged:")]
		partial void cbPresence_SelectionChanged (Foundation.NSObject sender);

		[Action ("txtMessageToSend_TextChanged:")]
		partial void txtMessageToSend_TextChanged (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (box != null) {
				box.Dispose ();
				box = null;
			}

			if (boxMessages != null) {
				boxMessages.Dispose ();
				boxMessages = null;
			}

			if (btnIsTyping != null) {
				btnIsTyping.Dispose ();
				btnIsTyping = null;
			}

			if (btnLogin != null) {
				btnLogin.Dispose ();
				btnLogin = null;
			}

			if (cbContacts != null) {
				cbContacts.Dispose ();
				cbContacts = null;
			}

			if (cbConversations != null) {
				cbConversations.Dispose ();
				cbConversations = null;
			}

			if (cbPresence != null) {
				cbPresence.Dispose ();
				cbPresence = null;
			}

			if (txt_login != null) {
				txt_login.Dispose ();
				txt_login = null;
			}

			if (txt_state != null) {
				txt_state.Dispose ();
				txt_state = null;
			}

			if (txtContactPresence != null) {
				txtContactPresence.Dispose ();
				txtContactPresence = null;
			}

			if (txt_password != null) {
				txt_password.Dispose ();
				txt_password = null;
			}

			if (txtMessagesExchanged != null) {
				txtMessagesExchanged.Dispose ();
				txtMessagesExchanged = null;
			}

			if (txtMessageToSend != null) {
				txtMessageToSend.Dispose ();
				txtMessageToSend = null;
			}

			if (txtSelectionInfo != null) {
				txtSelectionInfo.Dispose ();
				txtSelectionInfo = null;
			}
		}
	}
}
