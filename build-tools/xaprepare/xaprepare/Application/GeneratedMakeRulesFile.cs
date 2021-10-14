using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Xamarin.Android.Prepare
{
	partial class GeneratedMakeRulesFile : GeneratedFile
	{
		public GeneratedMakeRulesFile (string outputPath)
			: base (outputPath)
		{}

		public override void Generate (Context context)
		{
			Log.Todo ("Generate some help for `make help`");

			if (context == null)
				throw new ArgumentNullException (nameof (context));

			using (StreamWriter sw = Utilities.OpenStreamWriter (OutputPath)) {
				Generate (context, sw);
				sw.Flush ();
			}
		}

		string GetOutputFileName (Context context, string namePrefix)
		{
			return $"{namePrefix}-v{BuildInfo.XAVersion}.$(-num-commits-since-version-change)_{context.OS.Type}-{context.OS.Architecture}_$(GIT_BRANCH)_$(GIT_COMMIT)-$(CONFIGURATION)";
		}

		void Generate (Context context, StreamWriter sw)
		{
			string myPath = Path.Combine (BuildPaths.XAPrepareSourceDir, "Application", "GeneratedMakeRulesFile.cs");
			sw.WriteLine ( "#");
			sw.WriteLine ($"# Generated by {myPath}");
			sw.WriteLine ( "#");
			sw.WriteLine ();

			WriteVariable ("export OS_NAME", context.OS.Type);
			WriteVariable ("export OS_ARCH", context.OS.Architecture);
			WriteVariable ("export OS_ARCH_TRANSLATED", context.OS.ProcessIsTranslated ? "true" : "false");
			WriteVariable ("PRODUCT_VERSION", context.ProductVersion);
			WriteVariable ("MONO_SOURCE_FULL_PATH", Configurables.Paths.MonoSourceFullPath);

			// These must remain dynamic since the developer may change branches without re-running `prepare`
			string getGitBranchScript = Utilities.GetRelativePath (BuildPaths.XamarinAndroidSourceRoot, Path.Combine (Configurables.Paths.BuildToolsScriptsDir, "get-git-branch.sh"));
			WriteVariable ("GIT_BRANCH", $"$(shell LANG=C \"{getGitBranchScript}\" | tr -d '[[:space:]]' | tr -C a-zA-Z0-9- _)");
			WriteVariable ("GIT_COMMIT", $"$(shell LANG=C git log --no-color --first-parent -n1 --pretty=format:%h)");
			WriteVariable ("-num-commits-since-version-change", $"$(shell LANG=C git log {context.BuildInfo.CommitOfLastVersionChange}..HEAD --oneline 2>/dev/null | wc -l | sed 's/ //g')");

			WriteVariable ("ZIP_EXTENSION", context.OS.ZipExtension);
			WriteVariable ("ZIP_OUTPUT_BASENAME",    GetOutputFileName (context, "xamarin.android-oss"));
			WriteVariable ("_TEST_RESULTS_BASENAME", GetOutputFileName (context, "xa-test-results"));
			WriteVariable ("_BUILD_STATUS_BASENAME", GetOutputFileName (context, "xa-build-status"));

			WriteVariable ("ZIP_OUTPUT",               "$(ZIP_OUTPUT_BASENAME).$(ZIP_EXTENSION)");
			WriteVariable ("_BUILD_STATUS_ZIP_OUTPUT", "$(_BUILD_STATUS_BASENAME).$(ZIP_EXTENSION)");
			WriteVariable ("_TEST_RESULTS_ZIP_OUTPUT", "$(_TEST_RESULTS_BASENAME).$(ZIP_EXTENSION)");

			var allApiLevels     = new List <string> ();
			var allPlatformIDs   = new List <string> ();
			var allFrameworks    = new List <string> ();
			var apiLevels        = new List <string> ();
			var stableApiLevels  = new List <string> ();
			var frameworks       = new List <string> ();
			var stableFrameworks = new List <string> ();
			var platformIds      = new List <string> ();

			foreach (AndroidPlatform ap in BuildAndroidPlatforms.AllPlatforms) {
				string api = ap.ApiLevel.ToString ();

				allApiLevels.Add (api);
				allPlatformIDs.Add (ap.PlatformID);
				if (!String.IsNullOrEmpty (ap.Framework)) {
					allFrameworks.Add (ap.Framework);
					frameworks.Add (ap.Framework);
					if (ap.Stable)
						stableFrameworks.Add (ap.Framework);
				} else
					allFrameworks.Add ("-");

				if (!ap.Supported)
					continue;

				apiLevels.Add (api);
				platformIds.Add (ap.PlatformID);
				if (ap.Stable)
					stableApiLevels.Add (api);
			}

			var enabledJitAbis = AbiNames.AllJitAbis.Where (a => context.IsTargetJitAbiEnabled (a));
			var enabledHostAbis = AbiNames.AllHostAbis.Where (a => context.IsHostJitAbiEnabled (a));
			var enabledAotAbis = AbiNames.AllAotAbis.Where (a => context.IsTargetAotAbiEnabled (a));

			WriteVariable ("ALL_API_LEVELS",    ToValue (allApiLevels));
			WriteVariable ("ALL_PLATFORM_IDS",  ToValue (allPlatformIDs));
			WriteVariable ("ALL_FRAMEWORKS",    ToValue (allFrameworks));
			WriteVariable ("API_LEVELS",        ToValue (apiLevels));
			WriteVariable ("STABLE_API_LEVELS", ToValue (stableApiLevels));
			WriteVariable ("FRAMEWORKS",        ToValue (frameworks));
			WriteVariable ("STABLE_FRAMEWORKS", ToValue (stableFrameworks));
			WriteVariable ("ALL_JIT_ABIS",      ToValue (enabledJitAbis.ToList()));
			WriteVariable ("ALL_HOST_ABIS",     ToValue (enabledHostAbis.ToList ()));
			WriteVariable ("ALL_AOT_ABIS",      ToValue (enabledAotAbis.ToList ()));
			WriteVariable ("ANDROID_TOOLCHAIN_DIR", context.Properties.GetRequiredValue (KnownProperties.AndroidToolchainDirectory));
			if (context.MonoOptions != null && context.MonoOptions.Count > 0) {
				WriteVariable ("MONO_OPTIONS", ToValue (context.MonoOptions));
				sw.WriteLine ("export MONO_OPTIONS");
			}

			sw.WriteLine ("_MSBUILD_ARGS = \\");
			sw.WriteLine ($"\t/p:{KnownProperties.AndroidSupportedTargetJitAbis}={context.Properties.GetRequiredValue (KnownProperties.AndroidSupportedTargetJitAbis)} \\");
			sw.WriteLine ($"\t/p:{KnownProperties.AndroidSupportedHostJitAbis}={context.Properties.GetRequiredValue (KnownProperties.AndroidSupportedHostJitAbis)} \\");
			sw.WriteLine ($"\t/p:{KnownProperties.AndroidSupportedTargetAotAbis}={context.Properties.GetRequiredValue (KnownProperties.AndroidSupportedTargetAotAbis)}");

			OutputOSVariables (context, sw);

			WriteListVariable ("_BUNDLE_ZIPS_INCLUDE",         Configurables.Defaults.BundleZipsInclude);
			WriteListVariable ("_BUNDLE_ZIPS_EXCLUDE",         Configurables.Defaults.BundleZipsExclude);
			WriteListVariable ("_TEST_RESULTS_BUNDLE_INCLUDE", Configurables.Defaults.TestResultsBundleInclude);
			WriteListVariable ("_TEST_RESULTS_BUNDLE_EXCLUDE", Configurables.Defaults.TestResultsBundleExclude);
			WriteListVariable ("_BUILD_STATUS_BUNDLE_INCLUDE", Configurables.Defaults.BuildStatusBundleInclude);
			WriteListVariable ("_BUILD_STATUS_BUNDLE_INCLUDE", Configurables.Defaults.BuildStatusBundleIncludeConditional, true);
			WriteListVariable ("_BUILD_STATUS_BUNDLE_EXCLUDE", Configurables.Defaults.BuildStatusBundleExclude);

			sw.WriteLine ();
			sw.WriteLine (".PHONY: framework-assemblies");
			sw.WriteLine ("framework-assemblies:");

			string prevVersion = "v1.0";
			string monoFrameworksRoot = Path.Combine ("bin", "$(CONFIGURATION)", context.Properties.GetRequiredValue (KnownProperties.XABinRelativeInstallPrefix), Configurables.Paths.MonoAndroidFrameworksSubDir);
			for (int i = 0; i < apiLevels.Count; i++) {
				string curVersion = frameworks [i];
				string apiLevel = apiLevels [i];
				string platformId = platformIds [i];
				string redistFile = Path.Combine (monoFrameworksRoot, curVersion, "RedistList", "FrameworkList.xml");
				WriteRuleLine ($"grep -q {prevVersion} {redistFile}; \\");
				WriteRuleLine ( "if [ $$? -ne 0 ] ; then \\");
				WriteRuleLine ($"\trm -f {redistFile}; \\");
				WriteRuleLine ( "fi; \\");
				WriteRuleLine ( "$(call MSBUILD_BINLOG,Mono.Android,$(_SLN_BUILD)) src/Mono.Android/Mono.Android.csproj \\");
				WriteRuleLine ( "\t/p:Configuration=$(CONFIGURATION) $(_MSBUILD_ARGS) \\");
				WriteRuleLine ($"\t/p:AndroidApiLevel={apiLevel} /p:AndroidPlatformId={platformId} /p:AndroidFrameworkVersion={curVersion} \\");
				WriteRuleLine ($"\t/p:AndroidPreviousFrameworkVersion={prevVersion} || exit 1;");

				prevVersion = curVersion;
			}

			string firstApiLevel = apiLevels [0];
			string firstPlatformId = platformIds [0];
			string firstFramework = frameworks [0];
			string latestStableFramework = stableFrameworks [stableFrameworks.Count - 1];

			WriteMSBuildCall (
				fileToRemovePath: Path.Combine (monoFrameworksRoot, "v1.0", "Xamarin.Android.NUnitLite.dll"),
				projectPath: "src/Xamarin.Android.NUnitLite/Xamarin.Android.NUnitLite.csproj"
			);

			WriteMSBuildCall (
				fileToRemovePath: $"{monoFrameworksRoot}/{latestStableFramework}/Mono.Android.Export.*",
				projectPath: "src/Mono.Android.Export/Mono.Android.Export.csproj"
			);

			WriteMSBuildCall (
				fileToRemovePath: $"{monoFrameworksRoot}/{latestStableFramework}/OpenTK-1.0.*",
				projectPath: "build-tools/download-legacy-assemblies/download-legacy-assemblies.csproj"
			);
			sw.WriteLine ();

			if (context.RuleGenerators == null || context.RuleGenerators.Count == 0)
				return;

			foreach (RuleGenerator rg in context.RuleGenerators) {
				if (rg == null)
					continue;
				rg (this, sw);
			}

			void WriteMSBuildCall (string fileToRemovePath, string projectPath)
			{
				WriteRuleLine ($"rm -f {fileToRemovePath}");
				WriteRuleLine ($"$(call MSBUILD_BINLOG,NUnitLite,$(_SLN_BUILD)) $(MSBUILD_FLAGS) {projectPath} \\");
				WriteRuleLine ( "\t/p:Configuration=$(CONFIGURATION) $(_MSBUILD_ARGS) \\");
				WriteRuleLine ($"\t/p:AndroidApiLevel={firstApiLevel} /p:AndroidPlatformId={firstPlatformId} \\");
				WriteRuleLine ($"\t/p:AndroidFrameworkVersion={firstFramework} || exit 1;");
			}

			string ToValue (ICollection<string> list, string? separator = null)
			{
				return String.Join (separator ?? " ", list);
			}

			void WriteRuleLine (string line)
			{
				sw.Write ('\t');
				sw.WriteLine (line);
			}

			void WriteVariable (string name, string value)
			{
				sw.WriteLine ($"{name} = {value}");
			}

			void WriteListVariable (string name, ICollection <string> list, bool conditional = false)
			{
				if (list.Count == 0)
					return;

				if (!conditional)
					sw.Write ($"{name} =");

				foreach (string i in list) {
					string item = i.Trim ();
					if (String.IsNullOrEmpty (item))
						continue;

					if (conditional) {
						sw.WriteLine ($"ifneq ($(wildcard {item}),)");
						sw.WriteLine ($"{name} += {item}");
						sw.WriteLine ("endif");
						continue;
					}

					sw.WriteLine (" \\");
					sw.Write ($"\t{item}");
				}

				if (!conditional)
					sw.WriteLine ();
			}
		}

		partial void OutputOSVariables (Context context, StreamWriter sw);
	}
}
