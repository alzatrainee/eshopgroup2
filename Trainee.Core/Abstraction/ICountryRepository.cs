﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trainee.Core.DAL.Entities;

namespace Trainee.Core.Abstraction
{
    /// <summary>
    /// This repository provides CRUD operations for countries
    /// </summary>
    public interface ICountryRepository
    {
        /// <summary>
        /// Gets an IQueryable object of all countries
        /// </summary>
        /// <returns>An IQueryable of countries</returns>
        IQueryable<Country> GetCountries();
        /// <summary>
        /// Gets the specified Country
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Specified country if it exists</returns>
        Country GetCountry(int id);
        /// <summary>
        /// Adds a new country to database and returns it
        /// </summary>
        /// <param name="country">Country to be added</param>
        /// <returns>Added country</returns>
        Country AddCountry(Country country);
        /// <summary>
        /// Deletes Country with specified ID if it exists
        /// </summary>
        /// <param name="id">ID of a country to delete</param>
        void DeleteCountry(int id);
        /// <summary>
        /// Gets the specified Country
        /// </summary>
        /// <param name="code">Code of a country to be returned</param>
        /// <returns>Specified Country if it exists</returns>
        Country GetCountryByCode(string code);

    }
}
