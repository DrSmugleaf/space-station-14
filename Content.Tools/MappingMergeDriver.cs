﻿using System;

namespace Content.Tools
{
    internal static class MappingMergeDriver
    {
        /// %A: Our file
        /// %O: Origin (common, base) file
        /// %B: Other file
        /// %P: Actual filename of the resulting file
        public static void Main(string[] args)
        {
            var ourPath = args[0];
            var ours = new Map(ourPath);
            var based = args[1]; // On what?
            var other = new Map(args[2]);
            var fileName = args[3];

            ours.Merge(other);
            ours.Save(ourPath);

            Environment.Exit(0);
        }
    }
}
