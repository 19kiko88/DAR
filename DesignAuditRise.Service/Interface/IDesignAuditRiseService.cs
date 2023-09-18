using Converpedia.Models.Dsn;
using DesignAuditPedia.Models.Dsn.DesignCompare;
using NPOI.HPSF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalLibrary.Models;

namespace DesignAuditRise.Service.Interface
{
    public interface IDesignAuditRiseService
    {
        public Task<Result> GetExp3FileFromDSN(string dsnFilePath);
        public Task GetExp3FileFromZIP(string zipFilePath, string extractPath);
        public Task<Exp1> Exp1FileToEntity(string exp1Path);
        public Task<Exp1> GetFilterPage(Exp1 originalExp1Entity, string[] filterPage);
        public Task<DesignDiff> DsnCompare(Exp1 sourceExp1, Exp1 destinationExp1, Exp1 sourceFilterExp1, Exp1 destinationFilterExp1, string sourceDsnAlias, string destinationDsnAlias, bool oneWay);
    }
}
