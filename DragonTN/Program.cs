/*
    DragonNT is used by a medical office to nutralize dragon voice commands for use by GMT.
    Copyright (C) 2014  Nicholas Gillespie

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace DragonTN
{
    class Program
    {
        public static String MyCmdsLocation = "\\current\\MyCmds.dat";

        static void Main(string[] args)
        {
            string input_folder = ".";
            string output_folder = "./output";
            //Parse input variables
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())  //to lowercase to prevent issues with caps
                {
                    //Check to see if user asked for help
                    case "-h":
                    case "--help":
                        Help();
                        goto Finish;
                    //Check to see if user passed input directory
                    case "-i":
                    case "--input":
                        if (!CheckInput(args.Length, i)) goto Finish;
                        input_folder = args[i+1];
                        System.Console.WriteLine("Input directory set to: {0}",input_folder);
                        break;
                    //Check to see if user passed output directory
                    case "-o":
                    case "--output":
                        if (!CheckInput(args.Length, i)) goto Finish;
                        output_folder = args[i+1];
                        System.Console.WriteLine("Output directory set to: {0}",output_folder);
                        break;
                };
            }
            //Make sure output folder exist
            if (!System.IO.Directory.Exists(output_folder))
            {
                Console.WriteLine("Folder {0} doesn't exist, creating it now", output_folder);
                System.IO.Directory.CreateDirectory(output_folder);
            }
            ProcessData(input_folder, output_folder); //if logic is added between this and finish, then incase in if statement
            //End of Program
            Finish:
                System.Console.Write("Press any key to exit.");
                System.Console.ReadKey();
        }

        //Help Dialog when -h or --help flag is thrown
        static void Help()
        {
            System.Console.WriteLine
                (
                    "Dragon Template Neutralizer \r\n" +
                    "Written by Nicholas Gillespie [http://ollis.me]\r\n" +
                    "Released under the GNU License \r\n\r\n" +
                    "DragonTN goes though every folder in given directory\r\nlooking for dragon profiles. \r\nWhen found it removes all\r\ndragon commands leaving only the\r\nInsert # and text\r\n\r\n" +
                    "-h \t --help \t Displays this screen.\r\n" +
                    "-i \t --input \t Defines an input directory(Defaults to current).\r\n" +
                    "-o \t --output \t Defines an output directory(Defaults to [.\\output]).\r\n" +
                    "example 1: DragonTN -i \\\\dragon\\template\\location -o .\\mytemplate \r\n" +
                    "example 2: DragonTN -o .\\mytemplate \r\n"
                );
        }

        //Checks if user thows a flag like --input that they followed it up with something
        static bool CheckInput(int argLength, int currentPosition)
        {
            if (argLength <= currentPosition + 1) 
            { 
                System.Console.WriteLine("Flag paramater not set! Use -h for help");
                return false;
            }
            return true;
        }

        //Loop though input directory and parse all Dragon Data
        static bool ProcessData(string input_folder, string output_folder)
        {
            string _output_file_name;
            //Ref to current directory
            DirectoryInfo parent_directory = new DirectoryInfo(input_folder);
            if (!parent_directory.Exists) { System.Console.WriteLine("Directory: {0} is not found", input_folder); return false; }
            //Get directory info
            DirectoryInfo[] child_directories = parent_directory.GetDirectories();

            //Loop though direcotries and parse Dragon Command Files
            foreach (DirectoryInfo current_directory in child_directories)
            {
                _output_file_name = current_directory.Name + ".txt";
                string _input_file_location = current_directory.ToString() + MyCmdsLocation;
                System.Console.WriteLine("Processing: {0}", current_directory.Name);
                try
                {
                    //Finds keywords, ", extended and non-showing ascii chars, 3 consecutive spaces or more, data in brackets
                    //  Uppercase words running more than 4 in a row, paths (i.e. \user\local\bin)
                    string regex_command = "SendDragonKeys|\"|.?Sub Main|IBM Corp|([?\\177-\\377].)|[\x00-\x1f\x7f-\xff_~`$\"\\?]|\\B(\x20{3,})|{.*?}|[A-Z]{4,}(?![a-z.])|(\\\\\\w*)|}";
                    string current_line;
                    bool in_command = false;
                    StreamReader _input_file = new StreamReader(_input_file_location, Encoding.ASCII);
                    StreamWriter _output_file = new StreamWriter(output_folder + "\\" + _output_file_name, false, Encoding.UTF8);
                    Regex full_regex = new Regex(regex_command);
                    Regex insert_regex = new Regex("(Insert\x20(?!Jeff)\\w*)");//Grab Insert [word] ignore Insert Jeff its an un-needed test command
                    Regex end_regex = new Regex("End Sub");
                    //Reads one line at a time looking for triggers
                    while ((current_line = _input_file.ReadLine()) != null)
                    {
                        if (!in_command)//check to see if we are not in Insert command
                            if (insert_regex.IsMatch(current_line))//Check for Insert
                                in_command = true;
                            else //if Insert command is not found then continue to next line of the file
                                continue;
                        //Check to see if we are at the end of the command
                        if (end_regex.IsMatch(current_line))
                        { in_command = false; continue; } //if so throw flag and continue to next line of the file
                        //Run line though Regular Expression to remove un-needed junk
                        string working_line = full_regex.Replace(current_line, "");
                        //Need to ignore lines with only spaces
                        if (working_line.Length <= 1)
                            continue;
                        //Check to see if the word Insert exist in the line, if so add a line break before and after
                        if(insert_regex.IsMatch(working_line))
                        {
                            int i = working_line.IndexOf("Insert");
                            while (i != -1)
                            {
                                //working_line = Regex.Replace(working_line, "((?!Insert)[^a-z0-9 .]$)", "");//removes some random charaters on Inserts(i.e. Insert 23~)
                                working_line = working_line.Insert(i, "\r\n");
                                i = working_line.IndexOf("Insert", i+3);
                            }
                        }
                        //Avoid camlecasing that sometimes happens after cleanup
                        Regex camelcase_regex = new Regex("(\\B[A-Z]+?[a-z]+(?=[A-Z][^A-Z])|\\B[A-Z]+?[a-z]+(?=[^A-Z]))");
                        if (camelcase_regex.IsMatch(working_line))
                            working_line = camelcase_regex.Replace(working_line, "\r\n$1");
                        //write working line to file
                        _output_file.WriteLine(working_line);
                    }//while end
                    //Close output file and input file
                    Console.WriteLine("Finished Processing {0}", _output_file_name);
                    _input_file.Close();
                    _output_file.Close();
                } //try end
                catch (Exception e)
                {
                    System.Console.WriteLine("Error: {0}", e.Message);
                }//end catch
            }//end foreach
            return true;
        }
    }
}
