# README

## Synopsis

A C# WPF .NET Framework application to process Windows DAT files.

DAT files are UTF-16 text made by redirecting the output of the following
command in a PowerShell window to a destination file:

```Get-ChildItem -Recurse > FILE.dat```

***DATTool*** has 2 functions:

-# Compile each DAT file into its own ZIP file archive then use these archives
   to create a SQLite 3 database of file information
-# Provide a viewer to query the files database

## Usage

The application takes no arguments and can be run in either of these 2 ways:

-# Double-click the executable in Windows
-# At the command line run ```.\DATTool.exe```

**Directories:**
- *etc*
  - Directory for configuration files
- *src*
  - Directory for all source code

**Files:**
- *packages.config*
  - Packages configuration file
