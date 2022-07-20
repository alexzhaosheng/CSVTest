using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace QICTest.Test
{
    [TestClass]
    public class CSVTest
    {
        private Stream GetStream(string[] content)
        {
            MemoryStream ms = new MemoryStream();
            var buf = Encoding.UTF8.GetBytes(string.Join("\r", content));
            ms.Write(buf, 0, buf.Length);            
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        [TestMethod]
        public void TestLoadFromStream()
        {
            var testContent = new[]
            {
                "col1, col2, col3",
                "1,33.3,xyz",
                "2,35,abc",
                "3,39,rst"
            };

            using(var ms = GetStream(testContent))
            {
                var csv = new CSVFile(',');
                csv.Load(ms);

                Assert.AreEqual(3, csv.Length);

                Assert.AreEqual(3, csv.Colums.Length);
                Assert.AreEqual("col1", csv.Colums[0]);
                Assert.AreEqual("col3", csv.Colums[2]);
                
                Assert.AreEqual(33.3d, csv.GetDataInCell<double>(0, 1));
                Assert.AreEqual(33.3d, csv.GetDataInCell<double>(0, "col2"));

                Assert.AreEqual("rst", csv.GetDataInCell<string>(2, "col3"));
            }
        }

        [TestMethod]
        public void TestNewCVS()
        {
            var csv = new CSVFile(',');
            csv.SetColums(new string[] { "col1", "col2", "col3" });
            Assert.AreEqual(3, csv.Colums.Length);
            Assert.AreEqual("col1", csv.Colums[0]);

            csv.AddData(new string[] { "1", "33.3", "xyz" });
            Assert.AreEqual(1, csv.Length);
            Assert.AreEqual("xyz", csv.GetDataInCell<string>(0, 2));
            Assert.AreEqual(33.3d, csv.GetDataInCell<double>(0, 1));         
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void TestWrongCSV()
        {
            var testContent = new[]
           {
                "col1,col2,col3,col4",
                "1,33.3,xyz",
                "2,35,abc",
                "3,39,rst"
            };

            using (var ms = GetStream(testContent))
            {
                var csv = new CSVFile(',');
                csv.Load(ms);              
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void TestWrongData()
        {
            var csv = new CSVFile(',');
            csv.SetColums(new string[] { "col1", "col2", "col3" });
            csv.AddData(new[] { "abc", "123" });
        }
    }
}
