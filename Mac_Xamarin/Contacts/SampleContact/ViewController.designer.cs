// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace SampleContact
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSBox box { get; set; }

		[Outlet]
		AppKit.NSBox boxContact { get; set; }

		[Outlet]
		AppKit.NSButton btnLogin { get; set; }

		[Outlet]
		AppKit.NSButton btnRoster { get; set; }

		[Outlet]
		AppKit.NSPopUpButton cbContactsList { get; set; }

		[Outlet]
		AppKit.NSPopUpButton cbContactsListFound { get; set; }

		[Outlet]
		AppKit.NSTextField contact_displayname { get; set; }

		[Outlet]
		AppKit.NSTextField contact_firstname { get; set; }

		[Outlet]
		AppKit.NSTextField contact_id { get; set; }

		[Outlet]
		AppKit.NSTextField contact_jobtitle { get; set; }

		[Outlet]
		AppKit.NSTextField contact_lastname { get; set; }

		[Outlet]
		AppKit.NSTextField contact_nickname { get; set; }

		[Outlet]
		AppKit.NSImageView imgContactAvatar { get; set; }

		[Outlet]
		AppKit.NSImageView imgMyAvatar { get; set; }

		[Outlet]
		AppKit.NSTextField txt_displayname { get; set; }

		[Outlet]
		AppKit.NSTextField txt_firstname { get; set; }

		[Outlet]
		AppKit.NSTextField txt_jobtitle { get; set; }

		[Outlet]
		AppKit.NSTextField txt_lastname { get; set; }

		[Outlet]
		AppKit.NSTextField txt_login { get; set; }

		[Outlet]
		AppKit.NSTextField txt_nickname { get; set; }

		[Outlet]
		AppKit.NSSecureTextField txt_password { get; set; }

		[Outlet]
		AppKit.NSTextField txt_search { get; set; }

		[Outlet]
		AppKit.NSTextView txt_state { get; set; }

		[Outlet]
		AppKit.NSTextField txt_title { get; set; }

		[Action ("btnRosterClicked:")]
		partial void btnRosterClicked (Foundation.NSObject sender);

		[Action ("ConnectClicked:")]
		partial void ConnectClicked (Foundation.NSObject sender);

		[Action ("ContactsListSelectionChanged:")]
		partial void ContactsListSelectionChanged (Foundation.NSObject sender);

		[Action ("DeleteMyAvatarClicked:")]
		partial void DeleteMyAvatarClicked (Foundation.NSObject sender);

		[Action ("FoundListSelectionChanged:")]
		partial void FoundListSelectionChanged (Foundation.NSObject sender);

		[Action ("GetContactAvatarClicked:")]
		partial void GetContactAvatarClicked (Foundation.NSObject sender);

		[Action ("GetContactsClicked:")]
		partial void GetContactsClicked (Foundation.NSObject sender);

		[Action ("GetMyAvatarClicked:")]
		partial void GetMyAvatarClicked (Foundation.NSObject sender);

		[Action ("SearchContactClicked:")]
		partial void SearchContactClicked (Foundation.NSObject sender);

		[Action ("UpdateInfoClicked:")]
		partial void UpdateInfoClicked (Foundation.NSObject sender);

		[Action ("UpdateMyAvatarClicked:")]
		partial void UpdateMyAvatarClicked (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (box != null) {
				box.Dispose ();
				box = null;
			}

			if (boxContact != null) {
				boxContact.Dispose ();
				boxContact = null;
			}

			if (txt_password != null) {
				txt_password.Dispose ();
				txt_password = null;
			}

			if (btnLogin != null) {
				btnLogin.Dispose ();
				btnLogin = null;
			}

			if (btnRoster != null) {
				btnRoster.Dispose ();
				btnRoster = null;
			}

			if (cbContactsList != null) {
				cbContactsList.Dispose ();
				cbContactsList = null;
			}

			if (cbContactsListFound != null) {
				cbContactsListFound.Dispose ();
				cbContactsListFound = null;
			}

			if (contact_displayname != null) {
				contact_displayname.Dispose ();
				contact_displayname = null;
			}

			if (contact_firstname != null) {
				contact_firstname.Dispose ();
				contact_firstname = null;
			}

			if (contact_id != null) {
				contact_id.Dispose ();
				contact_id = null;
			}

			if (contact_jobtitle != null) {
				contact_jobtitle.Dispose ();
				contact_jobtitle = null;
			}

			if (contact_lastname != null) {
				contact_lastname.Dispose ();
				contact_lastname = null;
			}

			if (contact_nickname != null) {
				contact_nickname.Dispose ();
				contact_nickname = null;
			}

			if (imgContactAvatar != null) {
				imgContactAvatar.Dispose ();
				imgContactAvatar = null;
			}

			if (imgMyAvatar != null) {
				imgMyAvatar.Dispose ();
				imgMyAvatar = null;
			}

			if (txt_displayname != null) {
				txt_displayname.Dispose ();
				txt_displayname = null;
			}

			if (txt_firstname != null) {
				txt_firstname.Dispose ();
				txt_firstname = null;
			}

			if (txt_jobtitle != null) {
				txt_jobtitle.Dispose ();
				txt_jobtitle = null;
			}

			if (txt_lastname != null) {
				txt_lastname.Dispose ();
				txt_lastname = null;
			}

			if (txt_login != null) {
				txt_login.Dispose ();
				txt_login = null;
			}

			if (txt_nickname != null) {
				txt_nickname.Dispose ();
				txt_nickname = null;
			}

			if (txt_search != null) {
				txt_search.Dispose ();
				txt_search = null;
			}

			if (txt_state != null) {
				txt_state.Dispose ();
				txt_state = null;
			}

			if (txt_title != null) {
				txt_title.Dispose ();
				txt_title = null;
			}
		}
	}
}
