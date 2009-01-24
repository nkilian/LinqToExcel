﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MbUnit.Framework;
using System.IO;
using System.Data.SqlClient;
using log4net.Core;
using System.Data.OleDb;

namespace LinqToExcel.Tests
{
    [Author("Paul Yoder", "paulyoder@gmail.com")]
    [FixtureCategory("Integration")]
    [TestFixture]
    public class ColumnMappings_IntegrationTests : SQLLogStatements_Helper
    {
        IExcelRepository<Company> _repo;
        string _excelFileName;
        string _worksheetName;

        [TestFixtureSetUp]
        public void fs()
        {
            InstantiateLogger();
            string testDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string excelFilesDirectory = Path.Combine(testDirectory, "ExcelFiles");
            _excelFileName = Path.Combine(excelFilesDirectory, "Companies.xls");
            _worksheetName = "ColumnMappings";
        }

        [SetUp]
        public void s()
        {
            _repo = new ExcelRepository<Company>(_excelFileName);
        }

        [Test]
        public void all_properties_have_column_mappings()
        {
            _repo.AddMapping(x => x.Name, "Company Title");
            _repo.AddMapping(x => x.CEO, "Boss");
            _repo.AddMapping(x => x.EmployeeCount, "Number of People");
            _repo.AddMapping(x => x.StartDate, "Initiation Date");

            var companies = from c in _repo.Worksheet(_worksheetName)
                            where c.Name == "Taylor University"
                            select c;

            Company rival = companies.ToList()[0];
            Assert.AreEqual(1, companies.ToList().Count, "Result Count");
            Assert.AreEqual("Taylor University", rival.Name, "Name");
            Assert.AreEqual("Your Mom", rival.CEO, "CEO");
            Assert.AreEqual(400, rival.EmployeeCount, "EmployeeCount");
            Assert.AreEqual(new DateTime(1988, 7, 26), rival.StartDate, "StartDate");
        }

        [Test]
        public void some_properties_have_column_mappings()
        {
            _repo.AddMapping(x => x.CEO, "Boss");
            _repo.AddMapping(x => x.StartDate, "Initiation Date");

            var companies = from c in _repo.Worksheet(_worksheetName)
                            where c.Name == "Anderson University"
                            select c;

            Company rival = companies.ToList()[0];
            Assert.AreEqual(1, companies.ToList().Count, "Result Count");
            Assert.AreEqual("Anderson University", rival.Name, "Name");
            Assert.AreEqual("Your Mom", rival.CEO, "CEO");
            Assert.AreEqual(300, rival.EmployeeCount, "EmployeeCount");
            Assert.AreEqual(new DateTime(1988, 7, 26), rival.StartDate, "StartDate");
        }

        //Todo
        //It is desired to have the SqlException and message thrown instead of a general OleDbException when the
        //column name is incorrect, but I don't know how to do that yet
        //[ExpectedException(typeof(SqlException), "'The Big Cheese' column does not exist in 'Sheet1' worksheet")]
        [ExpectedException(typeof(OleDbException))]
        [Test]
        public void exception_on_property_with_column_mapping_used_in_where_clause_when_mapped_column_doesnt_exist()
        {
            _repo.AddMapping(x => x.CEO, "The Big Cheese");

            var companies = from c in _repo.Worksheet(_worksheetName)
                            where c.CEO == "Bugs Bunny"
                            select c;

            companies.GetEnumerator();
        }

        [Test]
        public void log_warning_when_property_with_column_mapping_not_in_where_clause_when_mapped_column_doesnt_exist()
        {
            _loggedEvents.Clear();
            _repo.AddMapping(x => x.CEO, "The Big Cheese");

            var companies = from c in _repo.Worksheet(_worksheetName)
                            select c;

            companies.GetEnumerator();
            int warningsLogged = 0;
            foreach (LoggingEvent logEvent in _loggedEvents.GetEvents())
            {
                if ((logEvent.Level == Level.Warn) &&
                    (logEvent.RenderedMessage == "'The Big Cheese' column that is mapped to the 'CEO' property does not exist in the 'Sheet1' worksheet"))
                    warningsLogged++;
            }
            Assert.AreEqual(1, warningsLogged);
        }
    }
}