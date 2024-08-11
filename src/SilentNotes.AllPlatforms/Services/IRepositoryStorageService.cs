// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.Models;

namespace SilentNotes.Services
{
    /// <summary>
    /// Interface of a service which can load/safe a note repository from the locale device
    /// </summary>
    public interface IRepositoryStorageService
    {
        /// <summary>
        /// Loads the note repository from the local storage, or gets the already loaded cached
        /// repository.
        /// </summary>
        /// <param name="repositoryModel">Retrieves the loaded repository, of null ini case of an error.</param>
        /// <returns>Returns the result whether the repository could be loaded.</returns>
        RepositoryStorageLoadResult LoadRepositoryOrDefault(out NoteRepositoryModel repositoryModel);

        /// <summary>
        /// Stores the note repository to the local storage.
        /// </summary>
        /// <param name="repositoryModel">The repository to store.</param>
        /// <returns>Returns true if the storage was successful, false otherwise.</returns>
        bool TrySaveRepository(NoteRepositoryModel repositoryModel);

        /// <summary>
        /// Clears the cache, so the next time the repository is loaded it is read from the local
        /// storage again.
        /// </summary>
        void ClearCache();

        /// <summary>
        /// Loads the note repository from the local storage and returns its binary content.
        /// This method is e.g. used to recover the file, to read the repository use
        /// <see cref="LoadRepositoryOrDefault(out NoteRepositoryModel)"/> instead.
        /// </summary>
        /// <returns>Content of repository, or null if no such file exists.</returns>
        byte[] LoadRepositoryFile();

        /// <summary>
        /// Tries to load the content of a file as respoitory.
        /// </summary>
        /// <param name="fileContent">Content of a loaded file.</param>
        /// <param name="repository">Receives the loaded repository, if the file contains a valid repository.</param>
        /// <returns>Returns true if the content is a valid repository, otherwise false.</returns>
        bool TryLoadRepositoryFromFile(byte[] fileContent, out NoteRepositoryModel repository);

        /// <summary>
        /// Gets the location (directory) where the repository is stored.
        /// </summary>
        /// <returns>Location of the repository.</returns>
        string GetLocation();
    }

    /// <summary>
    /// Enumeration of possible results after loading the repository.
    /// </summary>
    public enum RepositoryStorageLoadResult
    {
        /// <summary>
        /// The repository was successfully read from the cache or from the local storage.
        /// </summary>
        SuccessfullyLoaded,

        /// <summary>
        /// The repository does not yet exist on the local storage, a new empty repository was
        /// created.
        /// </summary>
        CreatedNewEmptyRepository,

        /// <summary>
        /// The repository exists on the local storage but is invalid and could not be loaded.
        /// </summary>
        InvalidRepository
    }
}
