/*
	Windows

        Invoke-WebRequest http://cakebuild.net/download/bootstrapper/windows -OutFile build.ps1
        .\build.ps1

	Linux

        curl -Lsfo build.sh http://cakebuild.net/download/bootstrapper/linux
        chmod +x ./build.sh
        ./build.sh

	OS X

        curl -Lsfo build.sh http://cakebuild.net/download/bootstrapper/osx
        chmod +x ./build.sh
        ./build.sh


	MacOSX 
	
		mono tools/Cake/Cake.exe --verbosity=diagnostic --target=libs
		mono tools/Cake/Cake.exe --verbosity=diagnostic --target=nuget
		
	Windows
		tools\Cake\Cake.exe --verbosity=diagnostic --target=libs
		tools\Cake\Cake.exe --verbosity=diagnostic --target=nuget
		tools\Cake\Cake.exe --verbosity=diagnostic --target=samples
*/	
#addin "Cake.Xamarin"

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

RunTarget("nuget-fixes");	// fix nuget problems on MacOSX

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
	.IsDependentOn ("libs-macosx")	
	//.IsDependentOn ("libs-windows")	
	.Does 
	(
		() => 
		{	
		}
	);

Task ("nuget-restore")
	.IsDependentOn ("nuget-fixes")
	.Does 
	(
		() => 
		{	
			Information("libs nuget_restore_settings.ToolPath = {0}", nuget_restore_settings.ToolPath);

			NuGetRestore 
				(
					"./external/Xamarin.Auth[]portable-bait-n-switch/source/Xamarin.Auth-Library.sln",
					nuget_restore_settings
				);
			NuGetRestore 
				(
					"./source/Salesforce.Library.sln",
					nuget_restore_settings
				);
			NuGetRestore 
				(
					"./samples/Samples.Salesforce.sln",
					nuget_restore_settings
				);
		}
	);

Task ("libs-macosx")
	.IsDependentOn ("nuget-fixes")
	.Does 
	(
		() => 
		{	
			CreateDirectory ("./output/");
			CreateDirectory ("./output/pcl/");
			CreateDirectory ("./output/android/");
			CreateDirectory ("./output/ios/");

			if ( ! IsRunningOnWindows() )
			{
				//-------------------------------------------------------------------------------------
				XBuild 
					(
						"./source/Salesforce.Library-MacOSX.sln",
						c => 
						{
							c.SetConfiguration("Release");
						}
					);
				XBuild 
					(
						"./source/Salesforce.Library-MacOSX.sln",
						c => 
						{
							c.SetConfiguration("Debug");
						}
					);
				XBuild 
					(
						"./source.nuget-references/Salesforce.Library-MacOSX.sln",
						c => 
						{
							c.SetConfiguration("Release");
						}
					);
				XBuild 
					(
						"./source.nuget-references/Salesforce.Library-MacOSX.sln",
						c => 
						{
							c.SetConfiguration("Debug");
						}
					);
				//-------------------------------------------------------------------------------------
					
				//-------------------------------------------------------------------------------------
				XBuild
					(
						"./source/Salesforce.Core/Salesforce.Core.csproj", 
						c => 
						{
							c.SetConfiguration("Release");
						}
					);
				XBuild
					(
						"./source/Salesforce.Core/Salesforce.Core.csproj", 
						c => 
						{
							c.SetConfiguration("Debug");
						}
					);

				XBuild
					(
						"./source.nuget-references/Salesforce.Core/Salesforce.Core.csproj", 
						c => 
						{
							c.SetConfiguration("Release");
						}
					);
				XBuild
					(
						"./source.nuget-references/Salesforce.Core/Salesforce.Core.csproj", 
						c => 
						{
							c.SetConfiguration("Debug");
						}
					);

				// not copying because this is link source assembly
				//-------------------------------------------------------------------------------------
				XBuild
					(
						"./source/Salesforce.Core.Portable/Salesforce.Core.Portable.csproj", 
						c => 
						{
							c.SetConfiguration("Release");
						}
					);
				XBuild
					(
						"./source/Salesforce.Core.Portable/Salesforce.Core.Portable.csproj", 
						c => 
						{
							c.SetConfiguration("Debug");
						}
					);
				XBuild
					(
						"./source.nuget-references/Salesforce.Core.Portable/Salesforce.Core.Portable.csproj", 
						c => 
						{
							c.SetConfiguration("Release");
						}
					);
				XBuild
					(
						"./source.nuget-references/Salesforce.Core.Portable/Salesforce.Core.Portable.csproj", 
						c => 
						{
							c.SetConfiguration("Debug");
						}
					);

				CopyFiles
					(
						"./source/Salesforce.Core.Portable/Salesforce.Core.Portable.csproj", 
						"./output/pcl/"
					);
				//-------------------------------------------------------------------------------------
				XBuild
					(
						"./source/Salesforce.Android/Salesforce.Android.csproj", 
						c => 
						{
							c.SetConfiguration("Release");
						}
					);
				XBuild
					(
						"./source/Salesforce.Android/Salesforce.Android.csproj", 
						c => 
						{
							c.SetConfiguration("Debug");
						}
					);
				XBuild
					(
						"./source.nuget-references/Salesforce.Android/Salesforce.Android.csproj", 
						c => 
						{
							c.SetConfiguration("Release");
						}
					);
				XBuild
					(
						"./source.nuget-references/Salesforce.Android/Salesforce.Android.csproj", 
						c => 
						{
							c.SetConfiguration("Debug");
						}
					);

				CopyFiles
					(
						"./source/Salesforce.Android/**/Release/Salesforce.dll", 
						"./output/android/"
					);
				//-------------------------------------------------------------------------------------
				XBuild
					(
						"./source/Salesforce.XamarinAndroid/Salesforce.XamarinAndroid.csproj", 
						c => 
						{
							c.SetConfiguration("Release");
						}
					);
				XBuild
					(
						"./source/Salesforce.XamarinAndroid/Salesforce.XamarinAndroid.csproj", 
						c => 
						{
							c.SetConfiguration("Debug");
						}
					);
				XBuild
					(
						"./source.nuget-references/Salesforce.XamarinAndroid/Salesforce.XamarinAndroid.csproj", 
						c => 
						{
							c.SetConfiguration("Release");
						}
					);
				XBuild
					(
						"./source.nuget-references/Salesforce.XamarinAndroid/Salesforce.XamarinAndroid.csproj", 
						c => 
						{
							c.SetConfiguration("Debug");
						}
					);

				CopyFiles
					(
						"./source/Salesforce.XamarinAndroid/**/Release/Salesforce.dll", 
						"./output/android/"
					);
				//-------------------------------------------------------------------------------------
				XBuild
					(
						"./source/Salesforce.iOS/Salesforce.iOS.csproj", 
						c => 
						{
							c.SetConfiguration("Release");
						}
					);
				XBuild
					(
						"./source/Salesforce.iOS/Salesforce.iOS.csproj", 
						c => 
						{
							c.SetConfiguration("Debug");
						}
					);
				XBuild
					(
						"./source.nuget-references/Salesforce.iOS/Salesforce.iOS.csproj", 
						c => 
						{
							c.SetConfiguration("Release");
						}
					);
				XBuild
					(
						"./source.nuget-references/Salesforce.iOS/Salesforce.iOS.csproj", 
						c => 
						{
							c.SetConfiguration("Debug");
						}
					);

				CopyFiles
					(
						"./source/Salesforce.iOS/**/Release/Salesforce.dll", 
						"./output/ios/"
					);
				//-------------------------------------------------------------------------------------
				XBuild
					(
						"./source/Salesforce.XamarinIOS/Salesforce.XamarinIOS.csproj", 
						c => 
						{
							c.SetConfiguration("Release");
						}
					);
				XBuild
					(
						"./source/Salesforce.XamarinIOS/Salesforce.XamarinIOS.csproj", 
						c => 
						{
							c.SetConfiguration("Debug");
						}
					);
				XBuild
					(
						"./source.nuget-references/Salesforce.XamarinIOS/Salesforce.XamarinIOS.csproj", 
						c => 
						{
							c.SetConfiguration("Release");
						}
					);
				XBuild
					(
						"./source.nuget-references/Salesforce.XamarinIOS/Salesforce.XamarinIOS.csproj", 
						c => 
						{
							c.SetConfiguration("Debug");
						}
					);

				CopyFiles
					(
						"./source/Salesforce.XamarinIOS/**/Release/Salesforce.dll", 
						"./output/ios/"
					);
				//-------------------------------------------------------------------------------------


			}

			return;
		}
	);


Task ("libs-windows")
	.IsDependentOn ("nuget-fixes")
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
			CreateDirectory ("./output/uwp/");
			
			if (IsRunningOnWindows ()) 
			{	
				MSBuild 
					(
						"./source/Salesforce.sln",
						c => 
						{
							c.SetConfiguration("Release")
							.SetPlatformTarget(PlatformTarget.x86);
						}
					);
				MSBuild 
					(
						"./source/Salesforce.sln",
						c => 
						{
							c.SetConfiguration("Debug")
							.SetPlatformTarget(PlatformTarget.x86);
						}
					);
				//-------------------------------------------------------------------------------------
				MSBuild
					(
						"./source/Salesforce.LinkSource/Salesforce.LinkSource.csproj", 
						c => 
						{
							c.SetConfiguration("Debug");
						}
					);
				MSBuild
					(
						"./source/Salesforce.LinkSource/Salesforce.LinkSource.csproj", 
						c => 
						{
							c.SetConfiguration("Release");
						}
					);
				//-------------------------------------------------------------------------------------
				MSBuild
					(
						"./source/Salesforce.Portable/Salesforce.Portable.csproj", 
						c => 
						{
							c.SetConfiguration("Release");
						}
					);
				CopyFiles
					(
						"./source/Salesforce.Portable/**/Release/Salesforce.dll", 
						"./output/pcl/"
					);
				//-------------------------------------------------------------------------------------
				MSBuild
					(
						"./source/Salesforce.XamarinAndroid/Salesforce.XamarinAndroid.csproj", 
						c => 
						{
							c.SetConfiguration("Release");
						}
					);
				CopyFiles
					(
						"./source/Salesforce.XamarinAndroid/**/Release/Salesforce.dll", 
						"./output/android/"
					);
				//-------------------------------------------------------------------------------------
				MSBuild
					(
						"./source/Salesforce.XamarinIOS/Salesforce.XamarinIOS.csproj", 
						c => 
						{
							c.SetConfiguration("Release");
						}
					);
				CopyFiles
					(
						"./source/Salesforce.XamarinIOS/**/Release/Salesforce.dll", 
						"./output/ios-unified/"
					);
				//-------------------------------------------------------------------------------------
				MSBuild
					(
						"./source/Salesforce.WindowsPhone8/Salesforce.WindowsPhone8.csproj", 
						c => 
						{
							c.SetConfiguration("Release")
							.SetPlatformTarget(PlatformTarget.x86);
						}
					);
				CopyFiles
					(
						"./source/Salesforce.WindowsPhone8/**/Release/Salesforce.dll", 
						"./output/wp80/"
					);
				//-------------------------------------------------------------------------------------
				MSBuild
					(
						"./source/Salesforce.WindowsPhone81/Salesforce.WindowsPhone81.csproj", 
						c => 
						{
							c.SetConfiguration("Release")
							.SetPlatformTarget(PlatformTarget.x86);
						}
					);
				CopyFiles
					(
						"./source/Salesforce.WindowsPhone81/**/Release/Salesforce.dll", 
						"./output/wp81/"
					);
				//-------------------------------------------------------------------------------------
				MSBuild
					(
						"./source/Salesforce.WinRTWindows81/Salesforce.WinRTWindows81.csproj", 
						c => 
						{
							c.SetConfiguration("Release");
						}
					);
				CopyFiles
					(
						"./source/Salesforce.WinRTWindows81/**/Release/Salesforce.dll", 
						"./output/win81/"
					);
				//-------------------------------------------------------------------------------------
				MSBuild
					(
						"./source/Salesforce.WinRTWindowsPhone81/Salesforce.WinRTWindowsPhone81.csproj", 
						c => 
						{
							c.SetConfiguration("Release");
						}
					);
				CopyFiles
					(
						"./source/Salesforce.WinRTWindowsPhone81/**/Release/Salesforce.dll", 
						"./output/wpa81/"
					);
				//-------------------------------------------------------------------------------------
				MSBuild
					(
						"./source/Salesforce.UniversalWindowsPlatform/Salesforce.UniversalWindowsPlatform.csproj", 
						c => 
						{
							c.SetConfiguration("Release");
						}
					);
				CopyFiles
					(
						"./source/Salesforce.UniversalWindowsPlatform/**/Release/Salesforce.dll", 
						"./output/uwp/"
					);
				//-------------------------------------------------------------------------------------

				} 

			return;
		}
	);

string[] sample_solutions = new []
{
	"samples/Traditional.Standard/references01projects/Providers/Xamarin.Auth.Samples.TraditionalStandard.sln",
	"samples/Traditional.Standard/references01projects/Providers/Xamarin.Auth.Samples.TraditionalStandard-MacOSX-Xamarin.Studio.sln",
	// "samples/bugs-triaging/component-2-nuget-package-migration-ArgumentNullException/Xamarin.Auth.ANE/Xamarin.Auth.ANE.sln",
	// "samples/Traditional.Standard/references01projects/Providers/old-for-backward-compatiblity/Xamarin.Auth.Sample.Android/Xamarin.Auth.Sample.Android.sln",
	// "samples/Traditional.Standard/references01projects/Providers/old-for-backward-compatiblity/Xamarin.Auth.Sample.iOS/Xamarin.Auth.Sample.iOS.sln",
	// "samples/Traditional.Standard/references01projects/Providers/Xamarin.Auth.Sample.WindowsPhone8/Component.Sample.WinPhone8.sln",
	// "samples/Traditional.Standard/references01projects/Providers/Xamarin.Auth.Sample.WindowsPhone81/Component.Sample.WinPhone81.sln",
	// "samples/Traditional.Standard/references01projects/Providers/Xamarin.Auth.Sample.XamarinAndroid/Component.Sample.Android.sln",
	// "samples/Traditional.Standard/references01projects/Providers/Xamarin.Auth.Sample.XamarinIOS/Component.Sample.IOS.sln",
	// "samples/Traditional.Standard/references01projects/Providers/Xamarin.Auth.Sample.XamarinIOS-Classic/Component.Sample.IOS-Classic.sln",
	// "samples/Traditional.Standard/references01projects/Providers/Xamarin.Auth.Samples.TraditionalStandard-MacOSX-Xamarin.Studio.sln",
	// "samples/Traditional.Standard/references01projects/Providers/Xamarin.Auth.Samples.TraditionalStandard-MacOSX-Xamarin.Studio.xxx.sln",
	// "samples/Traditional.Standard/references01projects/Providers/Xamarin.Auth.Samples.TraditionalStandard.sln",
	// "samples/Traditional.Standard/references02nuget/old-for-backward-compatiblity/Xamarin.Auth.Sample.Android/Xamarin.Auth.Sample.Android.sln",
	// "samples/Traditional.Standard/references02nuget/old-for-backward-compatiblity/Xamarin.Auth.Sample.iOS/Xamarin.Auth.Sample.iOS.sln",
	// "samples/Traditional.Standard/references02nuget/Xamarin.Auth.Sample.WindowsPhone8/Component.Sample.WinPhone8.sln",
	// "samples/Traditional.Standard/references02nuget/Xamarin.Auth.Sample.WindowsPhone81/Component.Sample.WinPhone81.sln",
	// "samples/Traditional.Standard/references02nuget/Xamarin.Auth.Sample.XamarinAndroid/Component.Sample.Android.sln",
	// "samples/Traditional.Standard/references02nuget/Xamarin.Auth.Sample.XamarinIOS/Component.Sample.IOS.sln",
	// "samples/Traditional.Standard/references02nuget/Xamarin.Auth.Sample.XamarinIOS-Classic/Component.Sample.IOS-Classic.sln",
	// "samples/Traditional.Standard/references02nuget/Xamarin.Auth.Samples.TraditionalStandard-MacOSX-Xamarin.Studio.sln",
	// "samples/Traditional.Standard/references02nuget/Xamarin.Auth.Samples.TraditionalStandard.sln",
	// "samples/Traditional.Standard/WindowsPhoneCrashMissingMethod-GetUI/WP8/Demo.sln",
	// "samples/Traditional.Standard/WindowsPhoneCrashMissingMethod-GetUI/WP8-XA/Demo.sln",
	// "samples/Xamarin.Forms/references01project/Evolve16Labs/04-Securing Local Data/Diary.sln",
	// "samples/Xamarin.Forms/references01project/Evolve16Labs/05-OAuth/ComicBook.sln",
	// "samples/Xamarin.Forms/references01project/Providers/XamarinAuth.XamarinForms.sln",
	// "samples/Xamarin.Forms/references02nuget/04-Securing Local Data/Diary.sln",
};

string[] build_configurations =  new []
{
	"Debug",
	"Release",
};

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

RunTarget (TARGET);