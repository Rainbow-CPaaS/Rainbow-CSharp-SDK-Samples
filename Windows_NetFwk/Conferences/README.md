![Rainbow](../../logo_rainbow.png)

 
# Rainbow-CSharp-SDK - Sample Conferences
---

This simple application permits to understand how to manage Conference features.

There is two different kind of Conference.
- Using your Personal Conference: It's based on PSTN Conference and it's necessary to have the correct permission .
- Using a Bubble with several members: it's also necessary to have the correct permission.

This permissions are independent. You can have access to one, both or none of them according your Rainbow licence. 

All information related to API here references the **Bubbles** class.

### Personal Conference
To check permission of Personal Conference, you can use **PersonalConferenceAllowed()**.

A Personal Conference is based on PSTN for the audio. So using classic device (deskphone, smartphone), you can access to it. 

Using the API you can join the Personal Conference in two different ways:
- Ask the server to call you back to a phone number you have specified
- Use a phone number to reach the conference then use a specific code to enter to it. The API permits to retrieve the list of phone numbers available and pass codes according your are moderator or a member  

For people not using Rainbow (Guest user) it's also possible to join the Personal Confrenece using an Public URL. The API permits to get this URL.

Pass codes and Public URL have no limit date. It's up to the end-user to reset them when necessary using the API

The Personal Conference is always hosted from a Bubble. So it's possible to add Video or Sharing in an active Personal Conference.

API specific to Personal Conference start with *PersonalConference* in **Bubbles** class. 


### WebRTC Conference
To check permission of WebRTC Conference, you can use **ConferenceAllowed()**.

A WebRTC Conference is based on WebRTC for the audio. So using classic device (deskphone, smartphone), you can access to it.

A WebRTC Conference is always started from a Bubble. It's possible to add Video or Sharing in an active webRTC Conference.

API specific to Conference start with *Conference* in **Bubbles** class. The word *WebRTC* is never used.


### Conference object
Once a Personal or a WebRTC Conference is active, event **ConferenceUpdated** is fired to know its status, participants and publishers.

This event contains a Conference object which is common whatever the kind of conference.

To know the bubble which contains this Conference, this method can be used **GetBubbleByConferenceIdFromCache(conferenceId)** or **GetBubbleIdByConferenceIdFromCache(conferenceId)**.

To know the bubble which contains the active Personal Conference, this method can be used **PersonalConferenceGetBubbleFromCache(conferenceId)** or **PersonalConferenceGetBubbleIdFromCache(conferenceId)**.  


### Common API to both kind of conference
- **ConferenceStart(...)** or **PersonalConferenceStart(...)** as shortcut for no WebRTC Conference
- **ConferenceStop(...)** or **PersonalConferenceStop(...)** as shortcut for no WebRTC Conference
- **ConferenceJoin(...)** or **PersonalConferenceJoin(...)** as shortcut for no WebRTC Conference
- **ConferenceMuteOrUnmute(...)** or **PersonalConferenceMuteOrUnmute(...)** as shortcut for no WebRTC Conference
- **ConferenceLockOrUnlock(...)** or **PersonalConferenceLockOrUnlock(...)** as shortcut for no WebRTC Conference
- **ConferenceMuteOrUnmutParticipant(...)** or **PersonalConferenceMuteOrUnmutParticipant(...)** as shortcut for no WebRTC Conference
- **ConferenceDropParticipant(...)** or **PersonalConferenceDropParticipant(...)** as shortcut for no WebRTC Conference


### Personal Conference API Specific
- **PersonalConferenceGetPhoneNumbers**
- **PersonalConferenceGetPassCodes**
- **PersonalConferenceResetPassCodes**
- **PersonalConferenceGetPublicUrl**
- **PersonalConferenceGenerateNewPublicUrl**

