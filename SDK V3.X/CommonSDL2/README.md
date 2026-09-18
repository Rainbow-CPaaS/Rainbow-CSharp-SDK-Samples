![Rainbow](../../logo_rainbow.png)
 
# Rainbow CSharp SDK examples - v3.x - Rainbow.Example.CommonSDL2

This project is used as common library to all examples for SDK V3.X using SDL2. It's compatible with Linux, Mac OS and Windows. 

It permits to centralize:
- same project / packages:
    - project reference to **Rainbow.Example.Common** (which also centralizes dependencies and objects)
    - package reference to **Rainbow.CSharp.SDK.Medias**
- same objects:
	- Stream: to define stream properties and behavior
	- StreamManager: to handle stream (close / start them according needs)
	- VideoFilter: static class to create valid FFmpeg filters (overlay, mosaic, ...)
	- Window:
        - create / destroy / hide / show
        - toggle full screen
        - set title 
        - to display video stream in windows: 
            - create/destroy/update texture
            - create/destroy/update renderer