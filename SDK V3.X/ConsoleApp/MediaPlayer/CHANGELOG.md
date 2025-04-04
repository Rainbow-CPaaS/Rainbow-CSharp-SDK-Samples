![Rainbow](./../../../logo_rainbow.png)

# Rainbow CSharp SDK v3 - MediaPlayer

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