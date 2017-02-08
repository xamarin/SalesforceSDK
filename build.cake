/*
    Windows

        Invoke-WebRequest http://cakebuild.net/download/bootstrapper/windows -OutFile build.ps1
        .\build.ps1

        tools\Cake\Cake.exe --verbosity=diagnostic --target=libs
        tools\Cake\Cake.exe --verbosity=diagnostic --target=nuget
        tools\Cake\Cake.exe --verbosity=diagnostic --target=samples

    Mac OS X

        curl -Lsfo build.sh http://cakebuild.net/download/bootstrapper/osx
        chmod +x ./build.sh
        #./build.sh
		
        mono tools/Cake/Cake.exe --verbosity=diagnostic --target=libs
        mono tools/Cake/Cake.exe --verbosity=diagnostic --target=nuget

    Linux

        curl -Lsfo build.sh http://cakebuild.net/download/bootstrapper/linux
        chmod +x ./build.sh
        ./build.sh
        
*/  
#addin "Cake.Xamarin"

//---------------------------------------------------------------------------------------
//mc++ 2017-01-26
// c# 6 string interpolation turned on
// Argument<bool>("experimental", true);
//---------------------------------------------------------------------------------------


//---------------------------------------------------------------------------------------
// some solutions with projects that use project references might experience on some
// systems (CI servers/ bots) following error:
//
// error MSB4018: System.IO.PathTooLongException: 
// The specified path, file name, or both are too long. The fully 
// qualified file name must be less than 260 characters, and the 
// directory name must be less than 248 characters. 
//
// Solution is to based on hostname to skip projects that reference projects in deeper
// external subfolders.
//
// solutions in ./source/ use Xamarin.Auth as projects references
string hostname_ci_windows_bot = "WIN-COMPONENTS";
string hostname = System.Environment.MachineName;

Information("Hostname = " + hostname);
//---------------------------------------------------------------------------------------


//---------------------------------------------------------------------------------------
var TARGET = Argument ("t", Argument ("target", Argument ("Target", "Default")));

FilePath nuget_tool_path = null;
FilePath cake_tool_path = null;

Task ("nuget-fixes")
    .Does 
    (
        () => 
        {
            if( ! IsRunningOnWindows() )
            {
                /*
                    Temporary fix for nuget bug MSBuild.exe autodetection on MacOSX and Linux

                    This target will be removed in the future! 

                    Executing: /Users/builder/Jenkins/workspace/Components-Generic-Build-Mac/CI/tools/Cake/../
                    nuget.exe restore "/Users/builder/Jenkins/workspace/Components-Generic-Build-Mac/CI/Xamarin.Auth/source/source/Xamarin.Auth-Library.sln" -Verbosity detailed -NonInteractive
                    MSBuild auto-detection: using msbuild version '4.0' from '/Library/Frameworks/Mono.framework/Versions/4.4.1/lib/mono/4.5'. 
                    Use option -MSBuildVersion to force nuget to use a specific version of MSBuild.
                    MSBuild P2P timeout [ms]: 120000
                    System.AggregateException: One or more errors occurred. 
                    ---> 
                    NuGet.CommandLineException: MsBuild.exe does not exist at '/Library/Frameworks/Mono.framework/Versions/4.4.1/lib/mono/4.5/msbuild.exe'.
                
                    NuGet Version: 3.4.4.1321

                    https://dist.nuget.org/index.html

                    Xamarin CI MacOSX bot uses central cake folder
                        .Contains("Components-Generic-Build-Mac/CI/tools/Cake");
                */
                nuget_tool_path = GetToolPath ("../nuget.exe");
                cake_tool_path = GetToolPath ("./Cake.exe");

                bool runs_on_xamarin_ci_macosx_bot = false;
                string path_xamarin_ci_macosx_bot = "Components-Generic-Build-Mac/CI/tools/Cake"; 

                string nuget_location = null;
                string nuget_location_relative_from_cake_exe = null;
                if (cake_tool_path.ToString().Contains(path_xamarin_ci_macosx_bot))
                {
                    runs_on_xamarin_ci_macosx_bot = true;
                    Information("Running on Xamarin CI MacOSX bot");
                }
                else
                {
                    Information("NOT Running on Xamarin CI MacOSX bot");
                }

                if (runs_on_xamarin_ci_macosx_bot)
                {
                    nuget_location = "../../tools/nuget.2.8.6.exe";
                    nuget_location_relative_from_cake_exe = "../nuget.2.8.6.exe";
                }
                else
                {
                    nuget_location = "./tools/nuget.2.8.6.exe";
                    nuget_location_relative_from_cake_exe = "../nuget.2.8.6.exe";
                }

                Information("nuget_location = {0} ", nuget_location);

                if ( ! FileExists (nuget_location))
                {
                    DownloadFile
                    (
                        @"https://dist.nuget.org/win-x86-commandline/v2.8.6/nuget.exe",
                        nuget_location
                    );
                }
                DirectoryPath path01 = MakeAbsolute(Directory("./"));
                string path02 = System.IO.Directory.GetCurrentDirectory();
                string path03 = Environment.CurrentDirectory;
                // Cake - WorkingDirectory??
                Information("path01         = {0} ", path01);
                Information("path02         = {0} ", path02);
                Information("path03         = {0} ", path03);
                Information("cake_tool_path = {0} ", cake_tool_path);
                nuget_tool_path = GetToolPath (nuget_location_relative_from_cake_exe);
            }

            Information("nuget_tool_path = {0}", nuget_tool_path);

            return;
        }
    );

RunTarget("nuget-fixes");   // fix nuget problems on MacOSX


//-----------------------------------------------------------------------------
// solutions in ./source/ use Xamarin.Auth as projects references
// might experience following error:
// error MSB4018: System.IO.PathTooLongException: 
// The specified path, file name, or both are too long. The fully 
// qualified file name must be less than 260 characters, and the 
// directory name must be less than 248 characters. 

string[] source_folders = new string[]
        {
            "source", 
            "source.nuget-references"
        };
//-----------------------------------------------------------------------------

string[] nuget_restore_solutions = new string[]
        {
            "./external/Xamarin.Auth/source/Xamarin.Auth-Library.sln",
            "./source/Salesforce.Library.sln",
            "./source/Salesforce.Library-MacOSX.sln",
            "./source.nuget-references/Salesforce.Library.sln",
            "./source.nuget-references/Salesforce.Library-MacOSX.sln",
            "./samples/Samples.Salesforce.sln",
        };
string[] source_solutions_macosx = new string[]
        {
            "Salesforce.Library-MacOSX",    // MacOSX Xamarin.Studio supported projects
        };
string[] source_solutions_windows = new string[]
        {
            "Salesforce.Library-MacOSX",    // MacOSX Xamarin.Studio supported projects
            "Salesforce.Library",           // Windows Visual Studio supported projects
        };
string[] sample_solutions = new []
        {
            "./samples/Samples.Salesforce.sln",
        };

string[] build_configurations =  new []
        {
            "Debug",
            "Release",
        };

NuGetRestoreSettings nuget_restore_settings = new NuGetRestoreSettings 
        { 
            ToolPath = nuget_tool_path,
            Verbosity = NuGetVerbosity.Detailed
        };

Task ("clean")
    .Does 
    (
        () => 
        {   
            // note no trailing backslash
            CleanDirectories ("./output");
            CleanDirectories("./source/**/bin");
            CleanDirectories("./source/**/obj");
            CleanDirectories("./source/**/Bin");
            CleanDirectories("./source/**/Obj");
            CleanDirectories("./samples/**/bin");
            CleanDirectories("./samples/**/obj");
            CleanDirectories("./samples/**/Bin");
            CleanDirectories("./samples/**/Obj");
        }
    );

Task ("distclean")
    .IsDependentOn ("clean")
    .Does 
    (
        () => 
        {   
            CleanDirectories("./**/packages");
            CleanDirectories("./**/Components");
        }
    );

Task ("rebuild")
    .IsDependentOn ("distclean")
    .IsDependentOn ("build")
    ;   

Task ("build")
    .IsDependentOn ("libs")
    .IsDependentOn ("samples")
    ;   

Task ("package")
    .IsDependentOn ("libs")
    .IsDependentOn ("nuget")
    ;   

Task ("libs")
    .IsDependentOn ("nuget-fixes")
    .IsDependentOn ("nuget-restore")    
    .Does 
    (
        () => 
        {   
            if ( ! IsRunningOnWindows() )
            {               
                RunTarget ("libs-macosx");
            }
            if ( IsRunningOnWindows() )
            {               
                // Mac OSX (Xamarin.Studio and Visual Studio for mac) 
                //      projects can be compiled on Windows
                //      but (always but)
                //      XBuild is used on MacOSX/Linux while on Windows MSBuild is used
                // RunTarget("libs-macosx");    
                RunTarget("libs-windows");
            }
        }
    );

Task ("nuget-restore")
    .IsDependentOn ("nuget-fixes")
    .Does 
    (
        () => 
        {   
            Information("libs nuget_restore_settings.ToolPath = {0}", nuget_restore_settings.ToolPath);

            foreach(string nrslns in nuget_restore_solutions)
            {
                NuGetRestore(nrslns, nuget_restore_settings);
            } 
        }
    );

Task ("libs-macosx")
    .IsDependentOn ("nuget-fixes")
    .IsDependentOn ("nuget-restore")
    .Does 
    (
        () => 
        {   
            CreateDirectory ("./output/");
            CreateDirectory ("./output/pcl/");
            CreateDirectory ("./output/android/");
            CreateDirectory ("./output/ios/");

            string project = null;

            //-------------------------------------------------------------------------------------
            foreach(string sf in source_folders)
            {
                foreach(string cfg in build_configurations)
                {
                    foreach(string sln in source_solutions_macosx)
                    {
                        string sf_sln = 
                                //$"./{sf}/Salesforce.Library.sln",  // C# 6 support missing
                                String.Format(@"./{0}/{1}.sln", sf, sln)
                                ;
                        XBuild 
                            (
                                sf_sln, // project or solution
                                c => 
                                { 
                                    c.SetConfiguration(cfg); // Debug or Release
                                } 
                            );
                    }
                }                   
            }   
            //-------------------------------------------------------------------------------------
                
            //-------------------------------------------------------------------------------------
            project = "Salesforce.Core";
            foreach(string sf in source_folders)
            {
                foreach(string cfg in build_configurations)
                {
                    string prj = 
                            //$"./{sf}}/{p}/{p}}.csproj"
                            String.Format(@"./{0}/{1}/{1}.csproj", sf, project)
                            ;
                    XBuild
                        (
                            prj, // project or solution
                            c => 
                            {
                                c.SetConfiguration(cfg); // Debug or Release
                            }
                        );      
                }
            }

            // not copying because this is link source assembly
            //-------------------------------------------------------------------------------------
            project = "Salesforce.Core.Portable";
            foreach(string sf in source_folders)
            {
                foreach(string cfg in build_configurations)
                {
                    string prj = 
                            //$"./{sf}}/{p}/{p}}.csproj"
                            String.Format(@"./{0}/{1}/{1}.csproj", sf, project)
                            ;
                    XBuild
                        (
                            prj, 
                            c => 
                            {
                                c.SetConfiguration(cfg);
                            }
                        );      
                }
            }

            CopyFiles
                (
                    "./source/" + project + "/**/Release/Salesforce.dll", 
                    "./output/pcl/"
                );
            //-------------------------------------------------------------------------------------
            project = "Salesforce.Android";
            foreach(string sf in source_folders)
            {
                foreach(string cfg in build_configurations)
                {
                    string prj = 
                            //$"./{sf}}/{p}/{p}}.csproj"
                            String.Format(@"./{0}/{1}/{1}.csproj", sf, project)
                            ;
                    XBuild
                        (
                            prj, 
                            c => 
                            {
                                c.SetConfiguration(cfg);
                            }
                        );      
                }
            }

            CopyFiles
                (
                    "./source/" + project + "/**/Release/Salesforce.dll", 
                    "./output/android/"
                );
            //-------------------------------------------------------------------------------------
            project = "Salesforce.XamarinAndroid";
            foreach(string sf in source_folders)
            {
                foreach(string cfg in build_configurations)
                {
                    string prj = 
                            //$"./{sf}}/{p}/{p}}.csproj"
                            String.Format(@"./{0}/{1}/{1}.csproj", sf, project)
                            ;
                    XBuild
                        (
                            prj, 
                            c => 
                            {
                                c.SetConfiguration(cfg);
                            }
                        );      
                }
            }

            CopyFiles
                (
                    "./source/" + project + "/**/Release/Salesforce.dll", 
                    "./output/android/"
                );
            //-------------------------------------------------------------------------------------
            project = "Salesforce.iOS";
            foreach(string sf in source_folders)
            {
                foreach(string cfg in build_configurations)
                {
                    string prj = 
                            //$"./{sf}}/{p}/{p}}.csproj"
                            String.Format(@"./{0}/{1}/{1}.csproj", sf, project)
                            ;
                    XBuild
                        (
                            prj, 
                            c => 
                            {
                                c.SetConfiguration(cfg);
                            }
                        );      
                }
            }

            CopyFiles
                (
                    "./source/" + project + "/**/Release/Salesforce.dll", 
                    "./output/ios/"
                );
            //-------------------------------------------------------------------------------------
            project = "Salesforce.XamarinIOS";
            foreach(string sf in source_folders)
            {
                foreach(string cfg in build_configurations)
                {
                    string prj = 
                            //$"./{sf}}/{p}/{p}}.csproj"
                            String.Format(@"./{0}/{1}/{1}.csproj", sf, project)
                            ;
                    XBuild
                        (
                            prj, 
                            c => 
                            {
                                c.SetConfiguration(cfg);
                            }
                        );      
                }
            }

            CopyFiles
                (
                    "./source/" + project + "/**/Release/Salesforce.dll", 
                    "./output/ios/"
                );
            //-------------------------------------------------------------------------------------

            return;
        }
    );


Task ("libs-windows")
    .IsDependentOn ("nuget-fixes")
    .IsDependentOn ("nuget-restore")
    .Does 
    (
        () => 
        {   
            CreateDirectory ("./output/");
            CreateDirectory ("./output/pcl/");
            CreateDirectory ("./output/android/");
            CreateDirectory ("./output/ios-unified/");
            CreateDirectory ("./output/ios/");
            CreateDirectory ("./output/wp80/");
            CreateDirectory ("./output/wp81/");
            CreateDirectory ("./output/wpa81/");
            CreateDirectory ("./output/win81/");
            CreateDirectory ("./output/winrt/");
            CreateDirectory ("./output/uap10.0/");
            
            string project = null;

            //-------------------------------------------------------------------------------------
            foreach(string sf in source_folders)
            {
				if 
					(
						hostname == hostname_ci_windows_bot
						&&
						source_folders.Contains("source")
					)
				{	
					//-----------------------------------------------------------------------------
					// solutions in ./source/ use Xamarin.Auth as projects references
					// might experience following error:
					// error MSB4018: System.IO.PathTooLongException: 
					// The specified path, file name, or both are too long. The fully 
					// qualified file name must be less than 260 characters, and the 
					// directory name must be less than 248 characters. 
					//-----------------------------------------------------------------------------
                    				
					continue;
				}
				
                foreach(string cfg in build_configurations)
                {
                    foreach(string sln in source_solutions_windows)
                    {
                        string sf_sln = 
                                //$"./{sf}/Salesforce.Library.sln",  // C# 6 support missing
                                String.Format(@"./{0}/{1}.sln", sf, sln)
                                ;
                        MSBuild 
                            (
                                sf_sln, // project or solution
                                c => 
                                { 
                                    c.SetConfiguration(cfg) // Debug or Release
                                    // Building Windows Phone application using MSBuild 64 bit 
                                    // is not supported. If you are using TFS build definitions, 
                                    // change the MSBuild platform to x86.
                                    .SetPlatformTarget(PlatformTarget.x86);
                                     
                                } 
                            );
                    }
                }                   
            }   
            //-------------------------------------------------------------------------------------
            project = "Salesforce.Core";
            foreach(string sf in source_folders)
            {
                foreach(string cfg in build_configurations)
                {
                    string prj = 
                            //$"./{sf}}/{p}/{p}}.csproj"
                            String.Format(@"./{0}/{1}/{1}.csproj", sf, project)
                            ;
                    MSBuild
                        (
                            prj, 
                            c => 
                            {
                                c.SetConfiguration(cfg);
                            }
                        );      
                }
            }
            //-------------------------------------------------------------------------------------
            project = "Salesforce.Core.Portable";
            foreach(string sf in source_folders)
            {
                foreach(string cfg in build_configurations)
                {
                    string prj = 
                            //$"./{sf}}/{p}/{p}}.csproj"
                            String.Format(@"./{0}/{1}/{1}.csproj", sf, project)
                            ;
                    MSBuild
                        (
                            prj, 
                            c => 
                            {
                                c.SetConfiguration(cfg);
                            }
                        );      
                }
            }

            CopyFiles
                (
                    "./source/" + project + "/**/Release/Salesforce.dll", 
                    "./output/pcl/"
                );
            //-------------------------------------------------------------------------------------
            project = "Salesforce.Android";
            foreach(string sf in source_folders)
            {
                foreach(string cfg in build_configurations)
                {
                    string prj = 
                            //$"./{sf}}/{p}/{p}}.csproj"
                            String.Format(@"./{0}/{1}/{1}.csproj", sf, project)
                            ;
                    MSBuild
                        (
                            prj, 
                            c => 
                            {
                                c.SetConfiguration(cfg);
                            }
                        );      
                }
            }

            CopyFiles
                (
                    "./source/" + project + "/**/Release/Salesforce.dll", 
                    "./output/android/"
                );
            //-------------------------------------------------------------------------------------
            project = "Salesforce.XamarinAndroid";
            foreach(string sf in source_folders)
            {
                foreach(string cfg in build_configurations)
                {
                    string prj = 
                            //$"./{sf}}/{p}/{p}}.csproj"
                            String.Format(@"./{0}/{1}/{1}.csproj", sf, project)
                            ;
                    MSBuild
                        (
                            prj, 
                            c => 
                            {
                                c.SetConfiguration(cfg);
                            }
                        );      
                }
            }

            CopyFiles
                (
                    "./source/" + project + "/**/Release/Salesforce.dll", 
                    "./output/android/"
                );
            //-------------------------------------------------------------------------------------
            project = "Salesforce.iOS";
            foreach(string sf in source_folders)
            {
                foreach(string cfg in build_configurations)
                {
                    string prj = 
                            //$"./{sf}}/{p}/{p}}.csproj"
                            String.Format(@"./{0}/{1}/{1}.csproj", sf, project)
                            ;
                    MSBuild
                        (
                            prj, 
                            c => 
                            {
                                c.SetConfiguration(cfg);
                            }
                        );      
                }
            }

            CopyFiles
                (
                    "./source/" + project + "/**/Release/Salesforce.dll", 
                    "./output/ios/"
                );
            //-------------------------------------------------------------------------------------
            project = "Salesforce.XamarinIOS";
            foreach(string sf in source_folders)
            {
                foreach(string cfg in build_configurations)
                {
                    string prj = 
                            //$"./{sf}}/{p}/{p}}.csproj"
                            String.Format(@"./{0}/{1}/{1}.csproj", sf, project)
                            ;
                    MSBuild
                        (
                            prj, 
                            c => 
                            {
                                c.SetConfiguration(cfg);
                            }
                        );      
                }
            }

            CopyFiles
                (
                    "./source/" + project + "/**/Release/Salesforce.dll", 
                    "./output/ios/"
                );
            //-------------------------------------------------------------------------------------
            project = "Salesforce.WindowsPhone8";
            foreach(string sf in source_folders)
            {
                foreach(string cfg in build_configurations)
                {
                    string prj = 
                            //$"./{sf}}/{p}/{p}}.csproj"
                            String.Format(@"./{0}/{1}/{1}.csproj", sf, project)
                            ;
                    MSBuild
                        (
                            prj, 
                            c => 
                            {
                                c.SetConfiguration(cfg)
                                .SetPlatformTarget(PlatformTarget.x86);
                            }
                        );      
                }
            }

            CopyFiles
                (
                    "./source/" + project + "/**/Release/Salesforce.dll", 
                    "./output/wp80/"
                );
            //-------------------------------------------------------------------------------------
            project = "Salesforce.WindowsPhone81";
            foreach(string sf in source_folders)
            {
                foreach(string cfg in build_configurations)
                {
                    string prj = 
                            //$"./{sf}}/{p}/{p}}.csproj"
                            String.Format(@"./{0}/{1}/{1}.csproj", sf, project)
                            ;
                    MSBuild
                        (
                            prj, 
                            c => 
                            {
                                c.SetConfiguration(cfg)
                                .SetPlatformTarget(PlatformTarget.x86);
                            }
                        );      
                }
            }

            CopyFiles
                (
                    "./source/" + project + "/**/Release/Salesforce.dll", 
                    "./output/wp81/"
                );
            //-------------------------------------------------------------------------------------
            project = "Salesforce.WinRTWindows81";
            foreach(string sf in source_folders)
            {
                foreach(string cfg in build_configurations)
                {
                    string prj = 
                            //$"./{sf}}/{p}/{p}}.csproj"
                            String.Format(@"./{0}/{1}/{1}.csproj", sf, project)
                            ;
                    MSBuild
                        (
                            prj, 
                            c => 
                            {
                                c.SetConfiguration(cfg);
                            }
                        );      
                }
            }

            CopyFiles
                (
                    "./source/" + project + "/**/Release/Salesforce.dll", 
                    "./output/win81/"
                );
            //-------------------------------------------------------------------------------------
            project = "Salesforce.WinRTWindowsPhone81";
            foreach(string sf in source_folders)
            {
                foreach(string cfg in build_configurations)
                {
                    string prj = 
                            //$"./{sf}}/{p}/{p}}.csproj"
                            String.Format(@"./{0}/{1}/{1}.csproj", sf, project)
                            ;
                    MSBuild
                        (
                            prj, 
                            c => 
                            {
                                c.SetConfiguration(cfg);
                            }
                        );      
                }
            }

            CopyFiles
                (
                    "./source/" + project + "/**/Release/Salesforce.dll", 
                    "./output/wpa81/"
                );
            //-------------------------------------------------------------------------------------
            project = "Salesforce.UniversalWindowsPlatform";
            foreach(string sf in source_folders)
            {
                foreach(string cfg in build_configurations)
                {
                    string prj = 
                            //$"./{sf}}/{p}/{p}}.csproj"
                            String.Format(@"./{0}/{1}/{1}.csproj", sf, project)
                            ;
                    MSBuild
                        (
                            prj, 
                            c => 
                            {
                                c.SetConfiguration(cfg);
                            }
                        );      
                }
            }

            CopyFiles
                (
                    "./source/" + project + "/**/Release/Salesforce.dll", 
                    "./output/uap10.0/"
                );
            CopyFiles
                (
                    "./source/" + project + "/**/Release/*.pri", 
                    "./output/uap10.0/"
                );
            CopyFiles
                (
                    "./source/" + project + "/**/Release/*.xr.xml", 
                    "./output/uap10.0/"
                );
            CopyFiles
                (
                    "./source/" + project + "/**/Release/*.xbf", 
                    "./output/uap10.0/"
                );
            //-------------------------------------------------------------------------------------

            return;
        }
    );

Task ("samples-nuget-restore")
    .Does 
    (
        () => 
        {
            foreach (string sample_solution in sample_solutions)
            {
                NuGetRestore(sample_solution, nuget_restore_settings); 
            }
            return;
        }
    );

Task ("samples")
    .IsDependentOn ("samples-nuget-restore")
    .IsDependentOn ("libs")
    .IsDependentOn ("nuget")
    .Does 
    (
        () => 
        {
            foreach (string sample_solution in sample_solutions)
            {
                foreach (string configuration in build_configurations)
                {
                    if ( IsRunningOnWindows() )
                    {
                        MSBuild
                            (
                                sample_solution, 
                                c => 
                                {
                                    c.SetConfiguration(configuration);
                                }
                            );
                    }
                    else
                    {
                        XBuild
                            (
                                sample_solution, 
                                c => 
                                {
                                    c.SetConfiguration(configuration);
                                }
                            );                      
                    }
                }
            }

            return;
        }
    );

Task ("nuget")
    .IsDependentOn ("libs")
    .Does 
    (
        () => 
        {
            NuGetPack 
                (
                    "./nuget/Salesforce.nuspec", 
                    new NuGetPackSettings 
                    { 
                        Verbosity = NuGetVerbosity.Detailed,
                        OutputDirectory = "./output/",        
                        BasePath = "./",
                        ToolPath = nuget_tool_path
                    }
                );                
        }
    );

Task ("externals")
    .Does 
    (
        () => 
        {
            return;
        }
    );


FilePath GetToolPath (FilePath toolPath)
{
    var appRoot = Context.Environment.GetApplicationRoot ();
     var appRootExe = appRoot.CombineWithFilePath (toolPath);
     if (FileExists (appRootExe))
     {
         return appRootExe;
     }
    throw new FileNotFoundException ("Unable to find tool: " + appRootExe); 
}


//=================================================================================================
// Put those 2 CI targets at the end of the file after all targets
// If those targets are before 1st RunTarget() call following error occusrs on 
//      *   MacOSX under Mono
//      *   Windows
// 
//  Task 'ci-osx' is dependent on task 'libs' which do not exist.
//
// Xamarin CI - Jenkins job targets
Task ("ci-osx")
    .IsDependentOn ("libs")
    .IsDependentOn ("nuget")
    //.IsDependentOn ("samples")
    ;
Task ("ci-windows")
    .IsDependentOn ("libs")
    .IsDependentOn ("nuget")
    //.IsDependentOn ("samples")
    ;   
//=================================================================================================

RunTarget (TARGET);