![Rainbow](./../../../logo_rainbow.png)

# Rainbow CSharp SDK v3 - MediaPlayer

##  [0.7.0]
- Configuration structure added
    - Add **autoPlayAudio**, **autoPlayVideo** and **autoPlaySharing**  ( before only **autoPlay** option was avaialble for video)  (in **stream.json** file) in order to start automatically the streaming specified when the file is 're)loaded and on application startup 
- Possibility to set/removet audio or video streams independently

##  [0.6.0]
- Configuration structure added
    - **streams**
        - **media**: "composition" is now supported
        - **videoComposition**: is now supported
        - **videoFilter**: is now supported
    - Add **autoPlay** option (in **stream.json** file) in order to start automatically the streaming specified when the file is loaded and on application startup
- Options added/updated:
    - **H** to display info/help menu (before it was **I**)
    - **I** for Input Stream selection (before it was **C**)
    - **L** to load / reload the file **streams.json**
    - **C** to cancel / stop current streaming

##  [0.5.0]
- Improve Window management (avoid crash when the window is resized / updated)
- Add **F** option to toggle to full screen (or the opposite)
- Keyboard actions performed on the video window are now redirected to the console window (which then has focus)
- These improvements need specific code relatif to the Windows platform

##  [0.0.1]
- Window, to display Video, has its size set to a default value at creation which can could be not adequate to display it correctly(bad ratio). You can resize it.

- Configuration structure not yet implemented / used
    - **streams**
        - **media**: "composition" is not supported
        - **videoComposition**: not supported
        - **videoFilter**: not supported