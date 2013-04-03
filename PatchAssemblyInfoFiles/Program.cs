using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PowerArgs;

namespace PatchAssemblyInfoFiles
{
  class Arguments
  {
    [ArgRequired(PromptIfMissing = false)]
    [ArgExistingDirectory]
    public string sourceBasePath { get; set; }

    [ArgDescription("version file which contains the version 1.2.3.4 string")]
    [ArgRequired(PromptIfMissing = false)]
    [ArgExistingFile]
    public string versionFile { get; set; }
    
    //[ArgDescription("version 1.2.3.4 can be supplied directly instead of version file")]
    //public string version { get; set; }

    [ArgDescription("file containing the user-friendly version information string (one liner)")]
    [ArgRequired(PromptIfMissing = false)]
    [ArgExistingFile]
    public string informalVersionFile { get; set; }

    //[ArgDescription("user-friendly version information string")]
    //public string informalVersion { get; set; }

    public string companyName { get; set; }
  }

  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("PatchAssemblyInfoFiles.exe");
      Console.WriteLine("This program searches in the specified dir + subdirs for AssemblyInfo.cs files");
      Console.WriteLine("And modifies the existing attributes with the specified values, or if the attributes are missing appends these");
      Console.WriteLine("");
      try
      {
        var parsed = Args.Parse<Arguments>(args);

        string versionString = System.IO.File.ReadAllText(parsed.versionFile).Trim().Replace(Environment.NewLine, "");
        Version version = new Version(versionString);

        string informalVersionString = System.IO.File.ReadAllText(parsed.informalVersionFile).Trim().Replace(Environment.NewLine, "");

        Console.Write(Environment.NewLine + Environment.NewLine);
        Console.WriteLine("Passed arguments (evaluated): ");
        Console.WriteLine(" - sourceBasePath=" + parsed.sourceBasePath);
        Console.WriteLine(" - version=" + version);
        Console.WriteLine(" - company=" + parsed.companyName);
        Console.WriteLine(" - informalversion=" + informalVersionString);
        Console.Write(Environment.NewLine);

        //find all assemblyinfo files
        var assemblyInfoFiles = Directory.GetFiles(parsed.sourceBasePath, "AssemblyInfo.cs", SearchOption.AllDirectories);
        Console.WriteLine(string.Format("Found {0} AssemblyInfo.cs files in source dir, going to patch these{1}", assemblyInfoFiles.Length, Environment.NewLine));

        foreach (string assemblyInfoFile in assemblyInfoFiles)
        {
          //open file, find attributes, replace content or add attributes
          FileInfo file = new FileInfo(assemblyInfoFile);
          file.IsReadOnly = false;//justin case

          string filecontents = System.IO.File.ReadAllText(assemblyInfoFile);

          if (!string.IsNullOrWhiteSpace(parsed.companyName))
          {
            string companyName = parsed.companyName.Replace('"', '\'');
            updateOrAddProperty(ref filecontents, "AssemblyCompany", companyName);
            updateOrAddProperty(ref filecontents, "AssemblyCopyright", "Copyright ©" + companyName + " " + DateTime.Now.Year);
          }
          
          updateOrAddProperty(ref filecontents, "AssemblyVersion", version.ToString());
          updateOrAddProperty(ref filecontents, "AssemblyFileVersion", version.ToString());
          updateOrAddProperty(ref filecontents, "AssemblyInformationalVersion", informalVersionString);

          System.IO.File.WriteAllText(assemblyInfoFile, filecontents);
          Console.WriteLine(" - patched " + assemblyInfoFile);
        }
      }
      catch (ArgException ex)
      {
        Console.WriteLine(ex.Message);
        Console.WriteLine(ArgUsage.GetUsage<Arguments>());
        Environment.Exit(1);
      }
      catch (Exception ex)
      { 
        Console.WriteLine(ex.ToString());
        Environment.Exit(1);
      }
    }

    private static void updateOrAddProperty(ref string filecontents, string propertyname, string value)
    {
      string pattern = "[assembly: "+propertyname+"(\"";
      if (filecontents.Contains(pattern))
      {
        updateFileContents(ref filecontents, 0, pattern, value);
      }
      else
      {
        //append new attribute
        filecontents += string.Format(@"{2}//added by PatchAssemblyInfoFiles.exe{2}[assembly: {0}(""{1}"")]", propertyname, value.Trim(), Environment.NewLine);
      }
    }

    private static void updateFileContents(ref string filecontents, int startIndex, string pattern, string value)
    {
      int startAt = filecontents.IndexOf(pattern, startIndex);
      if (startAt == -1)
        return;
        
      startAt += pattern.Length;
      int EndAd = filecontents.IndexOf("\")]", startAt);
      filecontents = filecontents.Remove(startAt, EndAd - startAt);//remove old crap
      filecontents = filecontents.Insert(startAt, value.Trim());//insert new stuff at start index

      updateFileContents(ref filecontents, EndAd, pattern, value);//maybe multiple values available?
    }
  }
}
