# Introduction
In response to our clinic's interest in making an electron cutout factor library, I created these two scripts to transform cutouts in the Varian Eclipse Treatment Planning System into .STL files. These form negatives about which Cerrobend (Wood's metal) can be poured, forming the prescribed cutout. This decreases opportunity for error (such as incorrect cutout printing distance, incorrect styrofoam cutout shape, incorrect cutout placement in frame), while requiring less hands-on time from the dosimetrist or physicist (just set it and let it print autonomously)!
The first script, a read-only (no script approval needed - yay!) script in Eclipse, converts the cutouts in Eclipse into an .SVG file and adds a printing base and two indexing arms.
The second script, a macro in FreeCAD, converts the .SVG file into a .STL. No user experience with FreeCAD is needed.

# Required Software
1. Varian Eclipse
2. FreeCAD
3. 3D Printer Slicer software (I used Ideamaker - I'm sure others will work).

# Procedure
## First Time Setup
1. Download FreeCAD. It's about half a gigabyte, and portable versions exist. My IT allowed it to be installed - yours should too.
2. Download the BlockToSVG.esapi.dll file in this repository (or build the .cs file yourself). Put it somewhere you can get to in Eclipse.
3. Download the Cutout_To_STL.FCMacro macro in this repository. To add it to FreeCAD, open FreeCAD, select Macro on the top left user bar, then note the user macros location at the bottom of the dialog:
<img width="655" height="530" alt="image" src="https://github.com/user-attachments/assets/d9df1d62-a9d4-4688-9ed2-6c32f0130700" />


4. Move the macro into this user macros location. Alternatively, just copy the text in the .FCMacro macro in this repository, then, in FreeCAD, select Macros > Create > name the file > paste > save > run.

## Eclipse Script
1. Enter a plan with electron cutout(s) in Eclipse. This script will export all cutouts on all fields in the plan, so if you've got two fields that use the same cutout, it will export it twice.
<img width="706" height="367" alt="image" src="https://github.com/user-attachments/assets/841185a8-366b-4539-969e-e64121436e32" />

2. Execute the script from Tools > Scripts > saved location of .esapi.dll file. Enter location to save .SVG files:
<img width="365" height="162" alt="image" src="https://github.com/user-attachments/assets/da5b696a-60ba-4de9-8c01-94330f228525" />

3. Hit OK. Next, enter the divergence scaling factor. As the cutout is not at isocenter, but at the source-to-slot distance, downscaling is needed to account for divergence of the field to isocenter. Yes, even if you're treating at extended SSD, the cutout sizes are given at isocenter in Eclipse. This is just (source-to-slot distance)/(source-to-axis distance), and should be 0.95. If you're using a Varian machine, you can leave this default.
<img width="368" height="160" alt="image" src="https://github.com/user-attachments/assets/1d5974bb-bed0-4b8e-a786-a9ee0e508c2e" />

4. The code will run and output your .SVG files into the prescribed folder, with the names "(MRN)-(plan name)-(field name)-Block.svg".
<img width="529" height="502" alt="image" src="https://github.com/user-attachments/assets/87633faa-aa81-4616-aa6b-261024c98f2e" />


## FreeCAD Script
1. Open FreeCAD.
2. Click Macro > Macros in the top left.
3. Select Cutout_to_STL.FCMacro, then the Execute button in the top right of the dialog.
4. Enter a wall thickness, if desired. To save on printing time, you can attempt to hollow out the apertures, so the 3D printer doesn't have to do it, so it prints faster. Do not make the wall thicker than the (smallest diameter of your cutout)/2 - in fact, don't get close to that. I haven't tested the minimum wall thickness to get good results, or infill. The hollowing function is very brittle, so this might cause the script to fail. If you just want to leave your cutouts solid (I'd recommend this for anything you'd put in a 6x6 cone), just hit enter and leave the number at 0.
5. An import dialog appears. Select one of your cutout SVG files.
6. Another dialog appears titled "Select module". Select "SVG as geometry (importSVG).
<img width="358" height="215" alt="image" src="https://github.com/user-attachments/assets/87bc2af3-78df-472a-a5b1-9557bab6caa4" />

7. Let the computer do its thing. An export window will automatically appear. Select a place to save the .STL file.
<img width="967" height="663" alt="image" src="https://github.com/user-attachments/assets/c2c4179d-faff-421a-8f6a-54ff79950140" />

## Slicer
1. Import your .STL file to the slicer software to generate instructions the 3D printer can understand.
2. Take your sliced file (probably gcode unless you have some proprietary stuff going on) to your 3D printer and print!
3. I have not set up any automation on this part as every clinic's got a different flow and printer here, but this is probably the easiest part.

ALL OUTPUTS MUST BE CHECKED FOR CORRECTNESS BY TRAINED PROFESSIONALS. THIS SOFTWARE DOES NOT CLAIM OR IMPLY ACCURACY OR SUITABILITY FOR CLINICAL USE. THE AUTHOR OF THIS CODE ASSUMES NO RESPONSIBILITY FOR THE USE OF THIS CODE.
