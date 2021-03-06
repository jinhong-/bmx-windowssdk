﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Inedo.BuildMasterExtensions.WindowsSdk.Recipes
{
    [Serializable]
    internal sealed class ProjectInfo
    {
        private static readonly Regex PathRegex = new Regex("[^" + Regex.Escape(new string(Path.GetInvalidPathChars())) + ":]+$");

        private char directorySeparator;
        private string scmPath;
        private string rootPath;
        private string relativePath;

        public ProjectInfo(char directorySeparator, string scmPath)
        {
            this.ConfigFiles = new List<string>();
            this.directorySeparator = directorySeparator;
            this.scmPath = scmPath;
            var match = PathRegex.Match(scmPath ?? string.Empty);
            this.relativePath = match.Value;
            this.rootPath = match.Success ? scmPath.Substring(0, match.Index) : scmPath;
        }

        public string ScmPath
        {
            get { return this.scmPath; }
        }
        public string ScmDirectoryName
        {
            get
            {
                int index = this.relativePath.LastIndexOf(this.directorySeparator);
                if (index >= 0)
                    return this.rootPath + this.relativePath.Substring(0, index);
                else
                    return this.rootPath;
            }
        }
        public string FileSystemPath
        {
            get { return this.relativePath.Replace(this.directorySeparator, '\\'); }
        }
        public string ProjectFileName
        {
            get
            {
                return this.relativePath.Substring(this.relativePath.LastIndexOf(this.directorySeparator) + 1);
            }
        }
        public string Name
        {
            get
            {
                var fileName = this.ProjectFileName;
                int index = fileName.LastIndexOf('.');
                if (index >= 0)
                    return fileName.Substring(0, index);
                else
                    return fileName;
            }
        }
        public bool IsWebApplication { get; set; }
        public List<string> ConfigFiles { get; private set; }
        public string DeploymentTarget { get; set; }
    }
}
