﻿using System;
using System.IO;
using System.Web;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web;

namespace Inedo.BuildMasterExtensions.WindowsSdk.MSBuild
{
    [ActionProperties(
        "Build ASP.NET Web or MVC Project",
        "Builds an ASP.NET Web Application or ASP.NET MVC project and applies .config transforms.")]
    [Tag(Tags.DotNet)]
    [CustomEditor(typeof(BuildAspNetProjectActionEditor))]
    public sealed class BuildAspNetProjectAction : MSBuildActionBase
    {
        /// <summary>
        /// Gets or sets the build configuration.
        /// </summary>
        [Persistent]
        public string ProjectBuildConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the project path.
        /// </summary>
        [Persistent]
        public string ProjectPath { get; set; }

        /// <summary>
        /// Gets or sets the additional arguments to pass to MSBuild.
        /// </summary>
        [Persistent]
        public string AdditionalArguments { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        /// <remarks>
        /// This should return a user-friendly string describing what the Action does
        /// and the state of its important persistent properties.
        /// </remarks>
        public override string ToString()
        {
            var sourceDir = Util.CoalesceStr(this.OverriddenSourceDirectory, "the default directory");
            var targetDir = Util.CoalesceStr(this.OverriddenTargetDirectory, "the default directory");
            var fileName = Path.GetFileName(this.ProjectPath);
            var config = this.ProjectBuildConfiguration;

            return string.Format("Build MVC Project {0} ({1}) from {2} to {3}", fileName, config, sourceDir, targetDir);
        }

        protected override void Execute()
        {
            this.ExecuteRemoteCommand(null);
        }

        protected override string ProcessRemoteCommand(string name, string[] args)
        {
            int exitCode = this.InvokeMSBuild(
                string.Format(
                    "\"{0}\" /t:Rebuild /p:Configuration={1};DeployOnBuild=True;BaseIntermediateOutputPath={2} {3}",
                    Path.Combine(this.Context.SourceDirectory, Path.GetFileName(this.ProjectPath)),
                    this.ProjectBuildConfiguration,
                    EnsureTrailingSlash(HttpUtility.UrlPathEncode(this.Context.TempDirectory)),
                    this.AdditionalArguments
                ),
                this.Context.SourceDirectory
            );

            // Output files are located in ..\{BuildConfiguration}\Package\PackageTmp
            string outputPath = Path.Combine(this.Context.TempDirectory, this.ProjectBuildConfiguration, @"Package\PackageTmp");

            if (!Directory.Exists(outputPath))
                throw new InvalidOperationException("There are no files in the expected output directory: " + outputPath);

            this.LogDebug("Moving files from {0} to target directory: {1} ", outputPath, this.Context.TargetDirectory);
            Util.Files.MoveFiles(outputPath, this.Context.TargetDirectory, true);

            return null;
        }

        private static string EnsureTrailingSlash(string path)
        {
            return path.TrimEnd('\\') + "\\";
        }
    }
}
