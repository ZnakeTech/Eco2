using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eco2.Models
{
    [TestClass]
    public class ThermostatsTests
    {
        const string TEST_XML_PATH = "./test-file.xml";

        [TestInitialize]
        public void Initialize()
        {
            File.Delete(TEST_XML_PATH);
            Thermostats.xmlPath = TEST_XML_PATH;
        }

        [TestCleanup]
        public void Cleanup()
        {
            File.Delete(TEST_XML_PATH);
        }

        [TestMethod]
        public void HandlesMissingFile()
        {
            var thermostats = Thermostats.Read();

            Assert.AreEqual(0, thermostats.thermostats.Count);
        }

        [TestMethod]
        public void CanReadFromFile()
        {
            File.WriteAllText(TEST_XML_PATH, @"
<Thermostats>
    <Thermostat>
        <Name>Test</Name>
    </Thermostat>
</Thermostats>");
            var thermostats = Thermostats.Read();

            Assert.AreEqual(1, thermostats.thermostats.Count);
            Assert.AreEqual("Test", thermostats.thermostats[0].Name);
        }

        [TestMethod]
        public void CanSaveToFile()
        {
            var from = DateTime.Now;
            var to = from.AddDays(2);
            var thermostat1 = new Thermostat
            {
                Name = "Thermostat1",
                SecretKey = "ABC",
                Uuid = "UUID123",
                Temperature = "encoded-temperature-1",
                UpdatedSetPointTemperature = Temperature.FromDegreesCelcius(22),
                HasUpdatedVacationPeriod = true,
                UpdatedVacationPeriod = new Period(from, to)
            };

            var thermostat2 = new Thermostat
            {
                Name = "Thermostat2",
                SecretKey = "DEF",
                Temperature = "encoded-temperature-2"
            };

            var thermostats = new Thermostats();
            thermostats.thermostats.Add(thermostat1);
            thermostats.thermostats.Add(thermostat2);

            thermostats.Write();

            var readThermostats = Thermostats.Read();
            Assert.AreEqual(2, readThermostats.thermostats.Count);
            var firstReadThermostat = readThermostats.thermostats[0];
            Assert.AreEqual("Thermostat1", firstReadThermostat.Name);
            Assert.AreEqual("ABC", firstReadThermostat.SecretKey);
            Assert.AreEqual("UUID123", firstReadThermostat.Uuid);
            Assert.AreEqual("encoded-temperature-1", firstReadThermostat.Temperature);
            Assert.AreEqual(22, firstReadThermostat.UpdatedSetPointTemperature.InDegreesCelcius);
            Assert.IsTrue(firstReadThermostat.HasUpdatedVacationPeriod);
            Assert.AreEqual(from, firstReadThermostat.UpdatedVacationPeriod.From);
            Assert.AreEqual(to, firstReadThermostat.UpdatedVacationPeriod.To);
        }

        [TestMethod]
        public void CanRemoveThermostatWithSerial()
        {
            var thermostat1 = new Thermostat
            {
                Serial = "Thermostat1"
            };

            var thermostat2 = new Thermostat
            {
                Serial = "Thermostat2"
            };

            var thermostats = new Thermostats();
            thermostats.thermostats.Add(thermostat1);
            thermostats.thermostats.Add(thermostat2);

            thermostats.RemoveThermostatWithSerial("Thermostat1");

            Assert.AreEqual(1, thermostats.thermostats.Count);
            Assert.AreEqual("Thermostat2", thermostats.thermostats[0].Serial);
        }

        [TestMethod]
        public void KnowsThatWeHaveNoSecretForMissingThermostat()
        {
            var thermostats = new Thermostats();

            Assert.IsFalse(thermostats.HasSecretAndUuidFor("Don't know this diddy"));
        }

        [TestMethod]
        public void KnowsWhenWeHaveNoSecretAndUuidForThermostat()
        {
            var thermostat = new Thermostat
            {
                Serial = "Thermostat1"
            };

            var thermostats = new Thermostats();
            thermostats.thermostats.Add(thermostat);

            Assert.IsFalse(thermostats.HasSecretAndUuidFor("Thermostat1"));
        }

        [TestMethod]
        public void KnowsThatSecretIsNotEnoughForSecretAndUuid()
        {
            var thermostat = new Thermostat
            {
                Serial = "Thermostat1",
                SecretKey = "ABC"
            };

            var thermostats = new Thermostats();
            thermostats.thermostats.Add(thermostat);

            Assert.IsFalse(thermostats.HasSecretAndUuidFor("Thermostat1"));
        }

        [TestMethod]
        public void KnowsThatUuidIsNotEnoughForSecretAndUuid()
        {
            var thermostat = new Thermostat
            {
                Serial = "Thermostat1",
                Uuid = "5D7000A0-0D45-41FC-B6AD-08CB8BC149B9",
            };

            var thermostats = new Thermostats();
            thermostats.thermostats.Add(thermostat);

            Assert.IsFalse(thermostats.HasSecretAndUuidFor("Thermostat1"));
        }

        [TestMethod]
        public void KnowsWhenWeHaveSecretAndUuidForThermostat()
        {
            var thermostat = new Thermostat
            {
                Serial = "Thermostat1",
                Uuid = "5D7000A0-0D45-41FC-B6AD-08CB8BC149B9",
                SecretKey = "ABC"
            };

            var thermostats = new Thermostats();
            thermostats.thermostats.Add(thermostat);

            Assert.IsTrue(thermostats.HasSecretAndUuidFor("Thermostat1"));
        }

        [TestMethod]
        public void GivesExistingThermostatBySerialWhenExists()
        {
            var thermostat = new Thermostat
            {
                Serial = "Thermostat1"
            };

            var thermostats = new Thermostats();
            thermostats.thermostats.Add(thermostat);

            Assert.AreSame(thermostat, thermostats.ThermostatWithSerial("Thermostat1"));
        }

        [TestMethod]
        public void CreatesNewThermostatWithSerialWhenNotExists()
        {
            var thermostats = new Thermostats();

            var newThermostat = thermostats.ThermostatWithSerial("Thermostat1");
            Assert.AreEqual("Thermostat1", newThermostat.Serial);
            Assert.AreSame(newThermostat, thermostats.thermostats[0]);
        }
    }
}
