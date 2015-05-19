# Fonter
### Matrix Font Data Generating for Assembler and C Projects

#####**Tour A** - new char for example font set
- for example click char A 
A is marked orange
- optionally press Clear 
Edit Grid is cleared
- draw an A in Edit Grid
(use left and right mouse button)
- press Save
A is added to set

#####**Tour B** - new font set
- enter a new set name in Set
- optionally press Load
error file not found
- continue with Tour A

#####**Hint**
- if you take the tours and running a compiled version of fonter
- be sure to copy dirs font and bitmap to same dir as fonter.exe
- if starting fonter with VisualStudio all is already in place

#####**Edit Grid**
- click (or hold) left mouse button to draw cell(s)
- click (or hold) right mouse button to clear cell(s)

##### **Char Field**
- green marked chars are present in set
- orange marked char is the active char
- single click a char to make it the active char
- double click a char to load it

##### **Set**
- enter name of font set to load or save
- all fonts are loaded or saved from %APPATH%\font

##### **Right from Set**

 - status messages
 green OK
 red ERROR

#####**Load**
- loads the font set specified in Set
- existing chars in the set will be marked green

#####**Width**
- width of the active char

#####**Height**

 - height of the active char

#####**Cell**

 - cell size of Edit Grid

#####**Clear**
- clear Edit Grid
(nothing is saved)

#####**Save**
- save current char to font set

#####**Delete**

 - delete current char from font set

#####**DB**
- use .DB format in data 
(mostly for assembler)

#####**0x**
- use 0x hex format in data

#####**$**
- use $ hex format in data

#####**16**
- data format width in byte

#####**Data Preview**
- shows data of current char
- data is copied to clipboard automatically

#####**Bitmap**
- a bitmap image of all chars of a font set is saved in %APPATH%\bitmap 
(when pressing Clip)

#####**Clip**
- copies the data of all chars of the font set to clipboard
(also see Bitmap)

#####**Exit**
- save settings and exit program

#####**Origin**
- TLY07 - TLY07R - TLX40
bit origin and order of data 
(see code)
- HXY
writes grid coordinates as you draw char
(for animated fonts)

#####**Preview and Control Window** 
- set dot size and gap
- set dot shape
- set dot color

[qrt@qland.de](mailto:qrt@qland.de)
