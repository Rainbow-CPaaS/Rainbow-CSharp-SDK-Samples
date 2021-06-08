![Rainbow](../logo_rainbow.png)

 
# Multiplatform Sample
---

This sample targets  targets **iOS**, **Android** but also **UWP** and **WPF** (experimental for this one)

Nearly 98% of the code is common to all this platform. 

MVVM architecture is used to create this sample as much as possible 

Features available for the moment:

- Conversations list:
    - Avatar of the Bubble ir provided or avatars members 
    - Nb unread messages indicator 
    - Presence level indicator (for peer to peer conversation)
    - Last message receivef or sent
    
- Conversation stream
    - Display list of messages received / sent
    - Display files thumbnail when they are available
    - Allow to send new message (not implemented in MVVM for the moment)
    
Elements implemented but not demonstrable 
- Themes: display is based on a selected theme