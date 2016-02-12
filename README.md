# DragonTN

DragonNT is a small console script used by a medical office to nutralize dragon voice commands for use by GMT.

####Help Output (-h or --help)
Dragon Template Neutralizer
Released under the GNU License

DragonTN goes though every folder in given directory
looking for dragon profiles.
When found it removes all
dragon commands leaving only the
Insert # and text

-h --help   Displays this screen.

-i --input  Defines an input directory(Defaults to current).

-o --output Defines an output directory(Defaults to [.\output]).

example 1: DragonTN -i \\dragon\template\location -o .\mytemplate

example 2: DragonTN -o .\mytemplate
