patchassemblyinfofiles
======================
Small tool to patch the AssemblyInfo.cs files with version information because all other options (MSBuild, nant task) for scripting did not yield good results.

Usage
======================
Usage: PatchAssemblyInfoFiles options

   OPTION                                TYPE      ORDER   DESCRIPTION
   -sourcebasepath (-s)                  String*           Path to the folder containing the sourcecode (subfolders)
   -versionfile (-v)                     String            version file which contains the version 1.2.3.4 string
   -version (-version)                   String            version 1.2.3.4 can be supplied directly instead of version file
   -informalversionfile (-i)             String            file containing the user-friendly version information string (one liner)
   -informalversion (-informalversion)   String            user-friendly version information string
   -companyname (-c)                     String            Company name to append to sourcecode [optional, if empty nothing done]