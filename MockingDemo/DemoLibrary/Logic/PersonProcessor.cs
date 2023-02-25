using DemoLibrary.Models;
using DemoLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DemoLibrary.Logic
{
    public class PersonProcessor : IPersonProcessor
    {
        ISqliteDataAccess _database;

        public PersonProcessor(ISqliteDataAccess database)
        {
            _database = database;
        }

        public PersonModel CreatePerson(string firstName, string lastName, string heightText)
        {
            CheckPersonParameters(firstName, lastName, heightText);

            PersonModel personModel = GetPersonModel(firstName, lastName, heightText);

            return personModel;
        }

        private PersonModel GetPersonModel(string firstName, string lastName, string heightText)
        {
            PersonModel personModel = new PersonModel();
            personModel.FirstName = firstName;
            personModel.LastName = lastName;
            double heightInInches = ConvertHeightTextToInches(heightText).heightInInches;
            personModel.HeightInInches = heightInInches;
            return personModel;
        }

        private void CheckPersonParameters(string firstName, string lastName, string heightText)
        {
            if (!IsValidName(firstName)) throw new ArgumentException("The value was not valid", "firstName");
            if (!IsValidName(lastName)) throw new ArgumentException("The value was not valid", "lastName");
            var height = ConvertHeightTextToInches(heightText);
            if (!height.isValid) throw new ArgumentException("The value was not valid", "heightText");
        }

        public List<PersonModel> LoadPeople()
        {
            string sql = "select * from Person";

            List<PersonModel> personModels = _database.LoadData<PersonModel>(sql);

            return personModels;
        }

        public List<PersonModel> LoadPeopleWithLongNames()
        {
            List<PersonModel> personModels = _database.LoadDataLongNames<PersonModel>();

            return personModels.Where(p => p.FirstName.Length > 10).ToList();
        }

        public void SavePerson(PersonModel person)
        {
            string sql = "insert into Person (FirstName, LastName, HeightInInches) " +
                "values (@FirstName, @LastName, @HeightInInches)";

            sql.Replace("@FirstName", $"'{person.FirstName}'");
            sql.Replace("@LastName", $"'{person.LastName}'");
            sql.Replace("@HeightInInches", $"'{person.HeightInInches}'");

            _database.SaveData(person, sql);
        }

        public void UpdatePerson(PersonModel person)
        {
            string sql = "update Person set FirstName = @FirstName, LastName = @LastName" +
                ", HeightInInches = @HeightInInches where Id = @Id";
            
            _database.UpdateData(person, sql);
        }

        public (bool isValid, double heightInInches) ConvertHeightTextToInches(string heightText)
        {
            bool correctFormatHeight = CheckHeightFormat(heightText);

            if (!correctFormatHeight) return (false, 0);

            string[] heightParts = heightText.Split(new char[] { '\'', '"' });
            bool hasFeetPart = int.TryParse(heightParts[0], out int feet) == true;
            bool hasInchesPart = double.TryParse(heightParts[1], out double inches) == true;
            bool hasBothFeetAndInchesPart = hasFeetPart && hasInchesPart;
            if (!hasBothFeetAndInchesPart) return (false, 0);

            double heightInInches = (feet * 12) + inches;

            return (true, heightInInches);
        }

        private static bool CheckHeightFormat(string heightText)
        {
            int feetMarkerLocation = heightText.IndexOf('\'');
            bool hasFeetMarker = feetMarkerLocation >= 0;

            int inchesMarkerLocation = heightText.IndexOf('"');
            bool hasInchesMarker = inchesMarkerLocation >= 0;

            bool correctOrderMarkers = inchesMarkerLocation >= feetMarkerLocation;

            bool correctFormatHeight = hasFeetMarker && hasInchesMarker && correctOrderMarkers;
            return correctFormatHeight;
        }

        private bool IsValidName(string name)
        {
            char[] invalidCharacters = "`~!@#$%^&*()_+=0123456789<>,.?/\\|{}[]'\"".ToCharArray();
            bool nameOnlyCorrectCharacters = name.IndexOfAny(invalidCharacters) < 0;

            bool nameCorrectLength = name.Length >= 2;

            return nameOnlyCorrectCharacters && nameCorrectLength;
        }
    }
}
