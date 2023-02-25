using DemoLibrary.Logic;
using DemoLibrary.Models;
using DemoLibrary.Utilities;
using Autofac.Extras.Moq;
using System.Collections.Generic;
using Moq;
using Xunit;
using System.Linq;
using System;

namespace MoqDemoTests.Utils
{
    public static class AutoMockExtension
    {
        public static PersonProcessor GetPersonProcessorWithFakeSaveData(this AutoMock mock, PersonModel person, string sql)
        {
            mock.Mock<ISqliteDataAccess>().Setup(_database => _database.SaveData(person, sql));

            PersonProcessor personProcessor = mock.Create<PersonProcessor>();
            return personProcessor;
        }

        public static PersonProcessor GetPersonProcessorWithFakeLoadData(this AutoMock mock)
        {
            // Make sure that when LoadData is called, it returns the content of GetSamplePeople,
            // instead of really going to the database.
            string sql = "select * from Person";
            mock.Mock<ISqliteDataAccess>()
                .Setup(_database => _database.LoadData<PersonModel>(sql))
                .Returns(mock.GetSamplePeople());

            // mock is now able to create a PersonProcessor  with a fake _database in it :
            PersonProcessor personProcessor = mock.Create<PersonProcessor>();
            return personProcessor;
        }

        public static PersonProcessor GetPersonProcessorWithFakeLoadDataLongNames(this AutoMock mock)
        {
            mock.Mock<ISqliteDataAccess>()
                .Setup(_database => _database.LoadDataLongNames<PersonModel>())
                .Returns(mock.GetSamplePeopleLongNames());

            PersonProcessor personProcessor = mock.Create<PersonProcessor>();
            return personProcessor;
        }

        public static void MakeSureSaveDataIsCalledExactlyOnce(this AutoMock mock, PersonModel person, string sql)
        {
            mock.Mock<ISqliteDataAccess>().Verify(_database => _database.SaveData(person, sql), Times.Exactly(1));
        }

        public static void MakeSureLoadPeopleCallsLoadData(this AutoMock mock, List<PersonModel> actualPersons)
        {
            List<PersonModel> expectedPersons = mock.GetSamplePeople();

            Assert.True(actualPersons != null);
            Assert.Equal(expectedPersons.Count, actualPersons.Count);

            foreach (var (actualPerson, expectedPerson) in actualPersons.Zip(expectedPersons, Tuple.Create))
            {
                Assert.Equal(actualPerson.FirstName, expectedPerson.FirstName);
                Assert.Equal(actualPerson.LastName, expectedPerson.LastName);
                Assert.Equal(actualPerson.HeightInInches, expectedPerson.HeightInInches);
            }
        }

        public static List<PersonModel> GetSamplePeople(this AutoMock mock)
        {
            List<PersonModel> personModels = new List<PersonModel>()
            {
                new PersonModel()
                {
                    FirstName = "mario",
                    LastName =  "buzza"
                },
                new PersonModel()
                {
                    FirstName = "valerie",
                    LastName =  "bouquet"
                },
                new PersonModel()
                {
                    FirstName = "julie",
                    LastName =  "faverjon"
                },
            };

            return personModels;
        }

        public static List<PersonModel> GetSamplePeopleLongNames(this AutoMock mock)
        {
            List<PersonModel> personModels = new List<PersonModel>()
            {
                new PersonModel()
                {
                    FirstName = "mario",
                    LastName =  "buzza"
                },
                new PersonModel()
                {
                    FirstName = "valerie-elisabeth",
                    LastName =  "bouquet"
                },
                new PersonModel()
                {
                    FirstName = "julie",
                    LastName =  "faverjon"
                },
            };

            return personModels;
        }
    }
}
