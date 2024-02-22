using Moq;
using Converpedia.Models.Dsn;
using DesignAuditRise.Service.Interface;
using DesignAuditRise.Service.OuterService.Interface;
using DesignAuditRise.Service.Models;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using DesignAuditPedia.Models.Dsn.DesignCompare;

namespace DAR.Tests.DesignAuditRiseService
{
    [TestClass()]
    public class DesignAuditRiseServiceTests
    {
        private readonly string _sampleFileDirectory = $@"{AppDomain.CurrentDomain.BaseDirectory}\test_files";        
        private static Mock<IZipService> _mockZipService = new Mock<IZipService>();
        private static Mock<IProtobufService> _mockProtobufService = new Mock<IProtobufService>();
        private static Mock<IFileWrapper> _mockFileWrapper = new Mock<IFileWrapper>();
        private static Mock<IConvertPediaOuterService> _mockConvertPediaService = new Mock<IConvertPediaOuterService>();
        private string _mockPath = @"X:\TestPath\Test.zip";
        private static Exp1 _mockExp1 = new Exp1();


        [ClassInitialize]
        public static void Init(TestContext testContext)
        {
            _mockExp1 = new Exp1()
            {
                Path = "Fake_Path",
                Schematics = new List<Schematic>()
                {
                    new Schematic(){
                        Name = "Schematic1",
                        Pages = new List<Page>(){ 
                            new Page() { Name = "S1-Page1" },
                            new Page() { Name = "S1-Page2" },
                            new Page() { Name = "S1-Page3" },
                            new Page() { Name = "S1-Page4" },
                            new Page() { Name = "S1-Page5" }
                        }
                    },
                    new Schematic(){
                        Name = "Schematic2",
                        Pages = new List<Page>(){ new Page() { Name = "S2-Page1" } }
                    }
                }
            };
        }

        /// <summary>
        /// 把前端沒有勾選的Page篩選掉，只保留前端有勾選的Pages。減少資料量
        /// </summary>
        [TestMethod()]
        public void SelectedPageFilter()
        {
            #region Arrange
            #endregion

            #region Act
            var mockSelectedPages = new string[] { "S2-Page1" };
            var service = new DesignAuditRise.Service.Implement.DesignAuditRiseService(null, null, null, null, null, null);
            var res = service.GetFilterPage(_mockExp1, mockSelectedPages).Result;

            var listFilterPages = new List<string>();
            res.Schematics.ForEach(c => c.Pages.ForEach(cc => listFilterPages.Add(cc.Name)));
            var filterPagesCount = listFilterPages.Where(c => mockSelectedPages.Contains(c)).Count();
            #endregion

            #region Assert
            Assert.AreEqual(mockSelectedPages.Length, filterPagesCount, "Exp1 Entity的Page數量與勾選數量不一致.");
            #endregion
        }

        /// <summary>
        /// 只從上傳的zip檔中取得特定檔案("part.exp", "cache.exp", "font.exp", "*.exp1", "*.png", "*.pdf")
        /// </summary>
        [TestMethod()]
        public void OrcadZipFileCheck()
        {
            var mockFiles = new string[] { "part.exp", "cache.exp", "font.exp", "RPL-DEMO-20221027_V19_EXP.exp1", "aa.png", "aa.pdf", "bb.exp2", "test.zip", "qq.bmp" };

            #region Arrange
            /*
             * 注入測試FileSystem，模擬System.IO的函式
             * Ref：
             * https://blog.csdn.net/lindexi_gd/article/details/106810660
             * https://www.ruyut.com/2023/05/testableio.system-io-abstractions.html             
             * 
            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    {fakeFilePath, new MockFileData("Test") }
                }
            );

            IFileWrapper fileWrapper = new FileWrapper(mockFileSystem);
            */

            _mockFileWrapper
                .Setup(_ => _.DirectoryFilesGet(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()))
                .Returns(mockFiles);
            #endregion

            #region Act
            var filterFiles = new string[] {};
            try
            {
                var service = new DesignAuditRise.Service.Implement.DesignAuditRiseService(null, null, null, null, _mockZipService.Object, _mockFileWrapper.Object);
                filterFiles = service.GetExp3FileFromZip(_mockPath, "").Result;
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            #endregion

            #region Assert
            //"bb.exp2", "test.zip", "qq.bmp"這3個檔案要被排除掉
            Assert.AreEqual(filterFiles.Length, 6, "未排除上傳的orcad zip中檔案名稱不符合規定(part.exp, cache.exp, font.exp, *.exp1, *.pdf, *.png)之檔案.");

            Assert.AreEqual(filterFiles.Where(c => c.EndsWith("exp1")).Any(), true, "上傳的orcad zip檔中必須要有exp1檔.");
            #endregion
        }

        /// <summary>
        /// 測試比對結果是否正常
        /// </summary>
        [TestMethod()]
        public void DsnCompareOk()
        {
            var resCompare = new DesignDiff();
            var sourceExp1 = @$"{_sampleFileDirectory}\exp1\source.exp1";
            var destExp1 = @$"{_sampleFileDirectory}\exp1\dest.exp1";
            var selectedSourcePageJson = @$"{_sampleFileDirectory}\json_file\selected_page\source.json";
            var selectedDestPageJson = @$"{_sampleFileDirectory}\json_file\selected_page\dest.json";

            #region Arrange
            var mockSourceExp1 = JsonConvert.DeserializeObject<Exp1>(File.ReadAllText(selectedSourcePageJson));
            //mockSourceExp1.Path不影響單元測試

            var mockDestExp1 = JsonConvert.DeserializeObject<Exp1>(File.ReadAllText(selectedDestPageJson));
            //mockDestExp1.Path不影響單元測試
            #endregion

            #region Act
            try
            {                
                var service = new DesignAuditRise.Service.OuterService.Implement.DesignCompareOterService();
                resCompare = service.SkrDsnCompare(sourceExp1, destExp1, "Design1", "Design2", mockSourceExp1, mockDestExp1).Content;
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            #endregion

            #region Assert
            Assert.AreEqual(resCompare.PartDiffs.Count, 16, "Part比對數量異常.");
            Assert.AreEqual(resCompare.NetDiffs.Count, 46, "Net比對數量異常.");
            #endregion
        }

        /// <summary>
        /// 要壓縮到ProtoBuffData.zip裡面的檔案檢核
        /// </summary>
        [TestMethod()]
        public void ProtoBuffZipFileCheck()
        {
            var mockFiles = new string[] {"filter_part.dat", "part.dat", "cache.dat", "font.dat", "aa.exp1", "aa.png", "aa.pdf", "bb.exp2", "test.zip", "qq.bmp" };
            var mockFilterPage = new string[] { "032.LGA1700 (FDI/DISPLAY)", "035.LGA1700 (Control)" };
            var jsonFile = @$"{_sampleFileDirectory}\json_file\dat\source.json";//讀取測試資料
            var mockSourceDat = JsonConvert.DeserializeObject<List<DsnLibrary.Models.PagePart>>(File.ReadAllText(jsonFile));//轉mock entity

            #region Arrange            
            _mockProtobufService.Setup(_ => _.ProtobufDatFileToList<DsnLibrary.Models.PagePart>(It.IsAny<string>())).ReturnsAsync(mockSourceDat);

            _mockFileWrapper.Setup(_ => _.FileExists(It.IsAny<string>())).Returns(true);

            _mockFileWrapper.Setup(_ => _.DirectoryEnumerateFiles(It.IsAny<string>())).Returns(mockFiles);

            _mockZipService.Setup(_ => _.Zip(It.IsAny<string>(), It.IsAny<string[]>()));
            #endregion

            #region Act
            var service = new DesignAuditRise.Service.Implement.DesignAuditRiseService(_mockConvertPediaService.Object, null, null, _mockProtobufService.Object, _mockZipService.Object, _mockFileWrapper.Object);
            var res = service.CreateProtobuffZip(_mockPath, mockFilterPage);
            #endregion

            #region Assert
            //filter_part.dat檔篩選檢核，只會出現勾選的兩個mock分頁
            Assert.AreEqual(res.Result.filterPartByPage.Where(c => !mockFilterPage.Contains(c)).Any(), false, "filter_part.dat檔案內容有誤，請確認篩選結果是否符合勾選Page.");

            Assert.AreEqual(res.Result.zipedFiles.Length, 6, "ProtoBuffData.zip中包含檔案名稱不符合規定(filter_part.dat, part.dat, cache.dat, font.dat, *.pdf, *.png)之檔案.");

            Assert.AreEqual(res.Result.zipedFiles.Where(c => c == "filter_part.dat").Any(), true, "ProtoBuffData.zip中必須要有filter_part.dat檔案.");
            #endregion
        }

        /// <summary>
        /// Page檢核
        /// </summary>
        [TestMethod()]
        public void SchematicPageCheck()
        {
            var schematics = new List<SchematicInfoModel>();
            var mockResult = new UniversalLibrary.Models.Result<Exp1>() { Success = true, Content = _mockExp1 };

            #region Arrange 
            _mockConvertPediaService.Setup(_ => _.Exp1FileToEntity(It.IsAny<string>())).ReturnsAsync(mockResult);
            #endregion

            #region Act
            try
            {
                var service = new DesignAuditRise.Service.Implement.DesignAuditRiseService(_mockConvertPediaService.Object, null, null, null, null, null);
                var res = service.GetSchematicInfo(_mockPath).Result;
                schematics = res.schematics;
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            #endregion

            #region Assert
            //page階層最多為1層
            Assert.AreEqual(schematics.Select(c => c.Children.Where(cc => cc.level != 2).Count()).Sum(s => s) == 0, true, "Page階層最多為1層.");
            //Schematics是否由小到大排序
            Assert.AreEqual(schematics.FirstOrDefault()?.Children[0].item == "S1-Page1" && schematics.FirstOrDefault()?.Children[4].item == "S1-Page5", true, "Page必須依照升冪排序.");
            #endregion            
        }
    }
}