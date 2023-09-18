namespace DesignAuditRise.Web
{
    public class AppSettings
    {
        public const string _OtherSettings = "OtherSettings";
        public const string _DsnServiceParameters = "DsnServiceParameters";
        public const string _PathSettings = "PathSettings";

        public class OtherSettings 
        { 
            public string[] DsnAlias { get; set; }
        }

        public class PathSettings
        {
            public string DsnFileUploadPath { get; set; }
            public string ZipFileUploadPath { get; set; }
            public string ExpFileTempPath { get; set; }
        }

        public class DsnServiceParameters
        {
            public string ServicePwd { get; set; }
            public int SendTimeout { get; set; }
            public string RemoteAddress { get; set; }
        }


    }
}
