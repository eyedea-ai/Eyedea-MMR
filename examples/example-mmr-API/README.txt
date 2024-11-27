Eyedentify SDK - MMR module library example
-----------------------------------------------
This file contains information about example of the Eyedentify SDK with MMR module.
The example demonstrates the basic usage of the Eyedentify's vehicle recognition module.

INPUT IMAGE REQUIREMENTS:
  The input image should satisfy the following criteria:
  - ALIGNED:          The front of the vehicle should be horizontally aligned.
  - 1:1 ASPECT RATIO: Image pixel aspect ratio should be 1:1.
  - WHOLE VEHICLE:    The vehicle is sufficiently distant from image borders.

BUILD AND RUN THE EXAMPLE:
  - Windows:
        1) Open example-mmr-API.vcxproj project in Visual Studio 2019 and higher.
        2) Build the project.
        3) Run created binary example-mmr-API.exe in the example folder.
  - Linux:
        1) CD to the example folder: cd Eyedentify-SDK/example/
        2) run: make clean
        3)      make
        4)      ./example-mmr-API
