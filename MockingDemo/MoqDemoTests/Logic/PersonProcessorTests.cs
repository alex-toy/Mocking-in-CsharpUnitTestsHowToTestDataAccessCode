using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.Moq;
using DemoLibrary.Logic;
using DemoLibrary.Models;
using DemoLibrary.Utilities;
using Xunit;

namespace MoqDemoTests.Logic
{
    public class PersonProcessorTests
    {
        [Theory]
        [InlineData("6'8\"", true, 80)]
        [InlineData("6\"8'", false, 0)]
        [InlineData("six'eight\"", false, 0)]
        public void ConvertHeightTextToInches_VariousOptions(
            string heightText, 
            bool expectedIsValid, 
            double expectedHeightInInches)
        {
            PersonProcessor processor = new PersonProcessor(null);

            var actual = processor.ConvertHeightTextToInches(heightText);

            Assert.Equal(expectedIsValid, actual.isValid);
            Assert.Equal(expectedHeightInInches, actual.heightInInches);
        }

        [Theory]
        [InlineData("Tim", "Corey", "6'8\"", 80)]
        [InlineData("Charitry", "Corey", "5'4\"", 64)]
        public void CreatePerson_Successful(string firstName, string lastName, string heightText, double expectedHeight)
        {
            PersonProcessor processor = new PersonProcessor(null);

            PersonModel expected = new PersonModel
            {
                FirstName = firstName,
                LastName = lastName,
                HeightInInches = expectedHeight,
                Id = 0
            };

            var actual = processor.CreatePerson(firstName, lastName, heightText);

            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.FirstName, actual.FirstName);
            Assert.Equal(expected.LastName, actual.LastName);
            Assert.Equal(expected.HeightInInches, actual.HeightInInches);

        }

        [Theory]
        [InlineData("Tim#", "Corey", "6'8\"", "firstName")]
        [InlineData("Charitry", "C88ey", "5'4\"", "lastName")]
        [InlineData("Jon", "Corey", "SixTwo", "heightText")]
        [InlineData("", "Corey", "5'11\"", "firstName")]
        public void CreatePerson_ThrowsException(string firstName, string lastName, string heightText, string expectedInvalidParameter)
        {
            PersonProcessor processor = new PersonProcessor(null);

            var ex = Record.Exception(() => processor.CreatePerson(firstName, lastName, heightText));

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
            if (ex is ArgumentException argEx)
            {
                Assert.Equal(expectedInvalidParameter, argEx.ParamName);
            }
        }

        [Fact]
        public void LoadPeople_ValidCall()
        {
            // Impossible to directly create PersonProcessor because we need a ISqliteDataAccess :
            //PersonProcessor personProcessor = new PersonProcessor();

            // As a consequence we create a mock :
            using (AutoMock mock = AutoMock.GetLoose())
            {
                // Make sure that when LoadData is called, it returns the content of GetSamplePeople,
                // instead of really going to the database.
                string sql = "select * from Person";
                mock.Mock<ISqliteDataAccess>()
                    .Setup(_database => _database.LoadData<PersonModel>(sql))
                    .Returns(GetSamplePeople());

                // mock is now able to create a PersonProcessor  with a fake _database in it :
                PersonProcessor personProcessor = mock.Create<PersonProcessor>();

                // We can now test that LoadPeople makes a call to the database :

                List<PersonModel> expectedPersons = GetSamplePeople();
                List<PersonModel> actualPersons = personProcessor.LoadPeople();

                Assert.True(actualPersons != null);
                Assert.Equal(expectedPersons.Count, actualPersons.Count);

                foreach (var (actualPerson, expectedPerson) in actualPersons.Zip(expectedPersons, Tuple.Create))
                {
                    Assert.Equal(actualPerson.FirstName, expectedPerson.FirstName);
                    Assert.Equal(actualPerson.LastName, expectedPerson.LastName);
                    Assert.Equal(actualPerson.HeightInInches, expectedPerson.HeightInInches);
                }
            }
        }

        [Fact]
        public void LoadPeopleWithLongNames_should_return_correctly()
        {
            using (AutoMock mock = AutoMock.GetLoose())
            {
                mock.Mock<ISqliteDataAccess>()
                    .Setup(_database => _database.LoadDataLongNames<PersonModel>())
                    .Returns(GetSamplePeopleLongNames());

                PersonProcessor personProcessor = mock.Create<PersonProcessor>();

                List<PersonModel> expectedPersons = GetSamplePeopleLongNames().Where(a => a.FirstName.Length > 10).ToList();
                List<PersonModel> actualPersons = personProcessor.LoadPeopleWithLongNames();

                Assert.True(actualPersons != null);
                Assert.Equal(expectedPersons.Count, actualPersons.Count);

                foreach (var (actualPerson, expectedPerson) in actualPersons.Zip(expectedPersons, Tuple.Create))
                {
                    Assert.Equal(actualPerson.FirstName, expectedPerson.FirstName);
                    Assert.Equal(actualPerson.LastName, expectedPerson.LastName);
                    Assert.Equal(actualPerson.HeightInInches, expectedPerson.HeightInInches);
                }
            }
        }

        private List<PersonModel> GetSamplePeople()
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

        private List<PersonModel> GetSamplePeopleLongNames()
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
