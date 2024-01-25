using System;
using System.Collections.Generic;

namespace DesignAuditRise.Service.Utility
{
    public static class Utils
    {
        public static string GetExp1FilePath(string directory)
        {
            string res = string.Empty;

            foreach (var item in Directory.GetFiles(directory))
            {
                if (Path.GetExtension(item).ToLower() == ".exp1")
                {
                    res = item;
                    break;
                }
            }

            return res;
        }

        public static string SecurityPathCombine(params string[] path)
        {
            var dir = path.FirstOrDefault()??""; //path[0].Replace(".", "").Replace("..", "").Replace("/", "");
                                             
            //相對路徑轉換絕對路徑
            var fullPath = Path.GetFullPath(Path.Combine(path));

            if (!fullPath.StartsWith(dir))
            {                    
                throw new Exception("Direct not found. Path Traversal Error.");
            }

            return fullPath;   
        }
    }
}
