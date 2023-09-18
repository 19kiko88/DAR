using CadTrans;
using Converpedia;
using Converpedia.Schematic;
using DesignAuditPedia.Dsn;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using UniversalLibrary.Models;

namespace DesignAuditRise.Service
{
    public class Class1
    {
        public async void qq()
        {
            DsnService QQ = new DsnService("");
            //var qqq = QQ.ToExp1File("");
            var pqp = await QQ.ToExp3File("D:\\Homer\\Project\\DesignAuditRise\\ignore\\base.dsn");
            var pqp112 = await QQ.ToExp1File("D:\\Homer\\Project\\DesignAuditRise\\ignore\\base.dsn");

            //Exp1Service PP = new Exp1Service();
            //var ppp = PP.Exp1FileToEntity("");
            //Converpedia.Schematic.ViewerService vs = new Converpedia.Schematic.ViewerService();
            


            var dsnFilePath = "D:\\Homer\\Project\\DesignAuditRise\\ignore\\base.dsn";
            Result result = new Result();
            if (!File.Exists(dsnFilePath))
            {
                result.Message = "File Not Found";
                //return result;
            }

            if (!Path.GetExtension(dsnFilePath)!.Equals(".dsn", StringComparison.OrdinalIgnoreCase))
            {
                result.Message = "File extension Error";
                //return result;
            }



            try
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(dsnFilePath);
                TranslateReport transReport = await CadTrans.DsnToExpAsync(File.ReadAllBytes(dsnFilePath), fileNameWithoutExtension, 3, servicePwd, async: false, "");
                if (transReport.Success)
                {
                    result.Message = transReport.ResultPath;
                    result.Success = true;
                    if (isDeleteFileAfterComplete)
                    {
                        CommonService.DeleteFile(dsnFilePath);
                    }
                }
                else
                {
                    result.Message = transReport.ErrorMessage;
                }
            }
            catch (Exception ex2)
            {
                Exception ex = (result.Exception = ex2);
                result.Message = ex.Message;
            }

            result.Success = true;
            //return result;
        }
    }
}