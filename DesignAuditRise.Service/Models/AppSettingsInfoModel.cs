namespace DesignAuditRise.Web
{
    public class AppSettingsInfoModel
    {
        public const string _OtherSettings = "OtherSettings";
        public const string _DsnServiceParameters = "DsnServiceParameters";
        public const string _PathSettings = "PathSettings";
        public const string _ConnectionStrings = "ConnectionStrings";
        public const string _UrlSettings = "UrlSettings";


        public class OtherSettings 
        { 
            public string[] DsnAlias { get; set; }
        }

        public class PathSettings
        {
            public string DsnFileUploadPath { get; set; }
            public string ZipFileUploadPath { get; set; }
            public string DatFileTempPath { get; set; }
        }

        public class DsnServiceParameters
        {
            public string ServicePwd { get; set; }
            public int SendTimeout { get; set; }
            public string RemoteAddress { get; set; }
        }

        public class ConnectionStrings
        {
            public string CAEDB01Connection { get; set; }
            public string CAEServiceConnection { get; set; }

        }

        public class UrlSettings
        {
            public string webSite { get; set; }
        }                    
    }
}
