// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace SampleConferences
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSBox box_inProgress { get; set; }

		[Outlet]
		AppKit.NSBox box_personal { get; set; }

		[Outlet]
		AppKit.NSBox box_webRTC { get; set; }

		[Outlet]
		AppKit.NSButton btnConferenceLock { get; set; }

		[Outlet]
		AppKit.NSButton btnConferenceMute { get; set; }

		[Outlet]
		AppKit.NSButton btnConferenceStop { get; set; }

		[Outlet]
		AppKit.NSButton btnLogin { get; set; }

		[Outlet]
		AppKit.NSButton btnParticipantDrop { get; set; }

		[Outlet]
		AppKit.NSButton btnParticipantMute { get; set; }

		[Outlet]
		AppKit.NSButton cbAsModerator { get; set; }

		[Outlet]
		AppKit.NSButton cbConferenceAvailable { get; set; }

		[Outlet]
		AppKit.NSButton cbIsPersonalConference { get; set; }

		[Outlet]
		AppKit.NSButton cbParticipantConnected { get; set; }

		[Outlet]
		AppKit.NSButton cbParticipantHold { get; set; }

		[Outlet]
		AppKit.NSButton cbParticipantModerator { get; set; }

		[Outlet]
		AppKit.NSButton cbParticipantMuted { get; set; }

		[Outlet]
		AppKit.NSPopUpButtonCell cbParticipantsList { get; set; }

		[Outlet]
		AppKit.NSButton cbPersonalConferenceAvailable { get; set; }

		[Outlet]
		AppKit.NSButton cbPersonalConferenceModerator { get; set; }

		[Outlet]
		AppKit.NSButton cbPersonalConferenceMuted { get; set; }

		[Outlet]
		AppKit.NSButton cbPublisherSharing { get; set; }

		[Outlet]
		AppKit.NSPopUpButtonCell cbPublishersList { get; set; }

		[Outlet]
		AppKit.NSButton cbPublisherVideo { get; set; }

		[Outlet]
		AppKit.NSTextField txt_login { get; set; }

		[Outlet]
		AppKit.NSSecureTextField txt_password { get; set; }

		[Outlet]
		AppKit.NSTextView txt_state { get; set; }

		[Outlet]
		AppKit.NSTextField txtBubbleId { get; set; }

		[Outlet]
		AppKit.NSTextField txtBubbleName { get; set; }

		[Outlet]
		AppKit.NSTextField txtBubbleOwner { get; set; }

		[Outlet]
		AppKit.NSTextField txtConferenceTalkers { get; set; }

		[Outlet]
		AppKit.NSTextField txtParticipant_PhoneNumber { get; set; }

		[Outlet]
		AppKit.NSTextField txtParticipantId { get; set; }

		[Outlet]
		AppKit.NSTextField txtParticipantJid { get; set; }

		[Outlet]
		AppKit.NSTextField txtPersonalConference_PhoneNumber { get; set; }

		[Outlet]
		AppKit.NSTextField txtPublisherId { get; set; }

		[Outlet]
		AppKit.NSTextField txtPublisherJid { get; set; }

		[Action ("btnConferenceLock_Clicked:")]
		partial void btnConferenceLock_Clicked (Foundation.NSObject sender);

		[Action ("btnConferenceMute_Clicked:")]
		partial void btnConferenceMute_Clicked (Foundation.NSObject sender);

		[Action ("btnConferenceStop_Clicked:")]
		partial void btnConferenceStop_Clicked (Foundation.NSObject sender);

		[Action ("btnLogin_Clicked:")]
		partial void btnLogin_Clicked (Foundation.NSObject sender);

		[Action ("btnParticipantDrop_Clicked:")]
		partial void btnParticipantDrop_Clicked (Foundation.NSObject sender);

		[Action ("btnParticipantMute_Clicked:")]
		partial void btnParticipantMute_Clicked (Foundation.NSObject sender);

		[Action ("btnPersonalConference_Join_Clicked:")]
		partial void btnPersonalConference_Join_Clicked (Foundation.NSObject sender);

		[Action ("btnPersonalConference_PassCodes_Clicked:")]
		partial void btnPersonalConference_PassCodes_Clicked (Foundation.NSObject sender);

		[Action ("btnPersonalConference_PhoneNumbers_Clicked:")]
		partial void btnPersonalConference_PhoneNumbers_Clicked (Foundation.NSObject sender);

		[Action ("btnPersonalConference_ResetPassCodes_Clicked:")]
		partial void btnPersonalConference_ResetPassCodes_Clicked (Foundation.NSObject sender);

		[Action ("btnPersonalConference_ResetUrl_Clicked:")]
		partial void btnPersonalConference_ResetUrl_Clicked (Foundation.NSObject sender);

		[Action ("btnPersonalConference_Start_Clicked:")]
		partial void btnPersonalConference_Start_Clicked (Foundation.NSObject sender);

		[Action ("btnPersonalConference_Url_Clicked:")]
		partial void btnPersonalConference_Url_Clicked (Foundation.NSObject sender);

		[Action ("cbParticipantsList_SelectionChanged:")]
		partial void cbParticipantsList_SelectionChanged (Foundation.NSObject sender);

		[Action ("cbPublishersList_SelectionChanged:")]
		partial void cbPublishersList_SelectionChanged (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (cbConferenceAvailable != null) {
				cbConferenceAvailable.Dispose ();
				cbConferenceAvailable = null;
			}

			if (box_inProgress != null) {
				box_inProgress.Dispose ();
				box_inProgress = null;
			}

			if (box_personal != null) {
				box_personal.Dispose ();
				box_personal = null;
			}

			if (box_webRTC != null) {
				box_webRTC.Dispose ();
				box_webRTC = null;
			}

			if (btnLogin != null) {
				btnLogin.Dispose ();
				btnLogin = null;
			}

			if (btnParticipantDrop != null) {
				btnParticipantDrop.Dispose ();
				btnParticipantDrop = null;
			}

			if (btnParticipantMute != null) {
				btnParticipantMute.Dispose ();
				btnParticipantMute = null;
			}

			if (cbAsModerator != null) {
				cbAsModerator.Dispose ();
				cbAsModerator = null;
			}

			if (cbIsPersonalConference != null) {
				cbIsPersonalConference.Dispose ();
				cbIsPersonalConference = null;
			}

			if (cbParticipantConnected != null) {
				cbParticipantConnected.Dispose ();
				cbParticipantConnected = null;
			}

			if (cbParticipantHold != null) {
				cbParticipantHold.Dispose ();
				cbParticipantHold = null;
			}

			if (cbParticipantModerator != null) {
				cbParticipantModerator.Dispose ();
				cbParticipantModerator = null;
			}

			if (cbParticipantMuted != null) {
				cbParticipantMuted.Dispose ();
				cbParticipantMuted = null;
			}

			if (cbParticipantsList != null) {
				cbParticipantsList.Dispose ();
				cbParticipantsList = null;
			}

			if (cbPersonalConferenceAvailable != null) {
				cbPersonalConferenceAvailable.Dispose ();
				cbPersonalConferenceAvailable = null;
			}

			if (cbPersonalConferenceModerator != null) {
				cbPersonalConferenceModerator.Dispose ();
				cbPersonalConferenceModerator = null;
			}

			if (cbPersonalConferenceMuted != null) {
				cbPersonalConferenceMuted.Dispose ();
				cbPersonalConferenceMuted = null;
			}

			if (cbPublisherSharing != null) {
				cbPublisherSharing.Dispose ();
				cbPublisherSharing = null;
			}

			if (cbPublishersList != null) {
				cbPublishersList.Dispose ();
				cbPublishersList = null;
			}

			if (cbPublisherVideo != null) {
				cbPublisherVideo.Dispose ();
				cbPublisherVideo = null;
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

			if (txtBubbleId != null) {
				txtBubbleId.Dispose ();
				txtBubbleId = null;
			}

			if (txtBubbleName != null) {
				txtBubbleName.Dispose ();
				txtBubbleName = null;
			}

			if (txtBubbleOwner != null) {
				txtBubbleOwner.Dispose ();
				txtBubbleOwner = null;
			}

			if (txtConferenceTalkers != null) {
				txtConferenceTalkers.Dispose ();
				txtConferenceTalkers = null;
			}

			if (txtParticipant_PhoneNumber != null) {
				txtParticipant_PhoneNumber.Dispose ();
				txtParticipant_PhoneNumber = null;
			}

			if (txtParticipantId != null) {
				txtParticipantId.Dispose ();
				txtParticipantId = null;
			}

			if (txtParticipantJid != null) {
				txtParticipantJid.Dispose ();
				txtParticipantJid = null;
			}

			if (txtPersonalConference_PhoneNumber != null) {
				txtPersonalConference_PhoneNumber.Dispose ();
				txtPersonalConference_PhoneNumber = null;
			}

			if (txtPublisherId != null) {
				txtPublisherId.Dispose ();
				txtPublisherId = null;
			}

			if (txtPublisherJid != null) {
				txtPublisherJid.Dispose ();
				txtPublisherJid = null;
			}

			if (btnConferenceMute != null) {
				btnConferenceMute.Dispose ();
				btnConferenceMute = null;
			}

			if (btnConferenceLock != null) {
				btnConferenceLock.Dispose ();
				btnConferenceLock = null;
			}

			if (btnConferenceStop != null) {
				btnConferenceStop.Dispose ();
				btnConferenceStop = null;
			}
		}
	}
}
