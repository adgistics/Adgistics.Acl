// Copyright (c) Adgistics Limited and others. All rights reserved. 
// The contents of this file are subject to the terms of the 
// Adgistics Development and Distribution License (the "License"). 
// You may not use this file except in compliance with the License.
// 
// http://www.adgistics.com/license.html
// 
// See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.IO;
using System.Reflection;

namespace Modules.Acl
{
    internal static class TestingFileResources
    {
        public static FileInfo Get(string fileName)
        {
            var codebasePath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            var testFilesPath = Path.Combine(codebasePath, "TestFiles");

            return new FileInfo(Path.Combine(testFilesPath, fileName));
        }

        public static Uri GetUri(string fileName)
        {
            return new Uri(Get(fileName).FullName);
        }
    }
}