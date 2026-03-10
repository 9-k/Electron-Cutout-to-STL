# Introduction
In response to our clinic's interest in making an electron cutout factor library, I created these two scripts to transform cutouts in the Varian Eclipse Treatment Planning System into .STL files. These form negatives about which Cerrobend (Wood's metal) can be poured, forming the prescribed cutout. This decreases opportunity for error (such as incorrect cutout printing distance, incorrect styrofoam cutout shape, incorrect cutout placement in frame), while requiring less hands-on time from the dosimetrist or physicist (just set it and let it print autonomously)!
The first script, a read-only (no script approval needed - yay!) script in Eclipse, converts the cutouts in Eclipse into an .SVG file and adds a printing base and two indexing arms.
The second script, a macro in FreeCAD, converts the .SVG file into a .STL. No user experience with FreeCAD is needed.

Most 6x6 and 10x10 prints are complete in under 2 hours. With the correct STL and printer settings, a 25x25 large scar boost negative will print in 7 hours (overnight).

# Required Software
1. Varian Eclipse
2. FreeCAD
3. 3D Printer Slicer software (I used Ideamaker - I'm sure others will work).

# Procedure
## First Time Setup
1. Download FreeCAD. It's about half a gigabyte, and portable versions exist. My IT allowed it to be installed - yours should too.
2. Download the BlockToSVG.esapi.dll file in this repository (or build the .cs file yourself). Put it somewhere you can get to in Eclipse.
3. Download the Cutout_To_STL.FCMacro macro in this repository. To add it to FreeCAD, open FreeCAD, select Macro > Macros on the top left user bar, then note the user macros location at the bottom of the dialog that appears:
<img width="655" height="530" alt="image" src="https://github.com/user-attachments/assets/d9df1d62-a9d4-4688-9ed2-6c32f0130700" />


4. Move the macro into this user macros location. Alternatively, just copy the text in the .FCMacro macro in this repository, then, in FreeCAD, select Macro > Macros > Create > name the file > paste > save > run.

## Eclipse Script
1. Enter a plan with electron cutout(s) in Eclipse. This script will export all cutouts on all fields in the plan, so if you've got two fields that use the same cutout, it will export it twice.
<img width="706" height="367" alt="image" src="https://github.com/user-attachments/assets/841185a8-366b-4539-969e-e64121436e32" />

2. Execute the script from Tools > Scripts > saved location of .esapi.dll file. Enter location to save .SVG files:
<img width="365" height="162" alt="image" src="https://github.com/user-attachments/assets/da5b696a-60ba-4de9-8c01-94330f228525" />

3. Hit OK. Next, enter the divergence scaling factor. As the cutout is not at isocenter, but at the source-to-slot distance, downscaling is needed to account for divergence of the field to isocenter. Yes, even if you're treating at extended SSD, the cutout sizes are given at isocenter in Eclipse. This is just (source-to-slot distance)/(source-to-axis distance), and should be 0.95 if you're using a Varian machine. Most can leave this default.
<img width="368" height="160" alt="image" src="https://github.com/user-attachments/assets/1d5974bb-bed0-4b8e-a786-a9ee0e508c2e" />

4. The code will run and output your .SVG files into the prescribed folder, with the names "(MRN)-(plan name)-(field name)-Block.svg".
<img width="529" height="502" alt="image" src="https://github.com/user-attachments/assets/87633faa-aa81-4616-aa6b-261024c98f2e" />


## FreeCAD Script
1. Open FreeCAD.
2. Click Macro > Macros in the top left.
3. Select CutoutToSTL.FCMacro, then the Execute button in the top right of the dialog.
4. A dialog asking for wall thickness, if desired, will appear. Entering a nonzero positive number will make FreeCAD attempt to hollow out the apertures so it prints faster. Do not make the wall thicker than the smallest diameter of your cutout / 2 - in fact, don't get close to that. I have found that 10 mm doesn't bend much for a 6x6, at least. 20% infill while printing is also sufficent. The hollowing function is very brittle, so this step may give bad results or cause the script to fail. If you just want to leave your cutouts solid, just hit enter and leave the number at 0.
5. Another dialog will appear, asking if you want to attempt a diverging cut. This can improve penumbra and make the 3d print easier to extract from the poured cerrobend cutout.
6. If yes, another dialog appears, asking for the source to slot distance. Default is 95 cm, which should be correct for Varian machines.
7. An import dialog appears. Select one of your cutout SVG files.
8. Another dialog appears titled "Select module". Select "SVG as geometry (importSVG).
<img width="358" height="215" alt="image" src="https://github.com/user-attachments/assets/87bc2af3-78df-472a-a5b1-9557bab6caa4" />

9. Let the computer do its thing. An export window will automatically appear. Select a place to save the .STL file.
<img width="718" height="469" alt="image" src="https://github.com/user-attachments/assets/90eca9a4-86b3-4e96-9b5b-f6010231272b" />

The cutout shown above employed hollowing to accelerate printing time and divergence for penumbra and better pour mould extraction!

10. Repeat for all needed cutouts in the plan.

## Slicer
1. Import your .STL file(s) to the slicer software to generate instructions the 3D printer can understand.
2. Take your sliced file(s) (probably gcode unless you have some proprietary stuff going on) to your 3D printer and print!
3. I have not set up any automation on this part as every clinic's got a different flow and printer here, but this is probably the easiest part.

ALL OUTPUTS MUST BE CHECKED FOR CORRECTNESS BY TRAINED PROFESSIONALS. THIS SOFTWARE DOES NOT CLAIM OR IMPLY ACCURACY OR SUITABILITY FOR CLINICAL USE. THE AUTHOR OF THIS CODE ASSUMES NO RESPONSIBILITY FOR THE USE OF THIS CODE.
