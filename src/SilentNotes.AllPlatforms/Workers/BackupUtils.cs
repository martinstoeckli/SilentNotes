// Copyright © 2025 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using SilentNotes.Models;
using SilentNotes.Services;

namespace SilentNotes.Workers
{
    /// <summary>
    /// Contains functions to create and restore a backup.
    /// </summary>
    public static class BackupUtils
    {
        /// <summary>
        /// Lets the user pick an output folder and saves a backup to the selected folder.
        /// </summary>
        /// <param name="folderPickerService">Service which can let the user pick a folder.</param>
        /// <param name="repositoryService">Service which can load the repository.</param>
        /// <returns>Task for async calls.</returns>
        public static async Task CreateBackup(IFolderPickerService folderPickerService, IRepositoryStorageService repositoryService)
        {
            if (await folderPickerService.PickFolder())
            {
                // Create a zip file, so that in future, attachements can be added as well.
                var repositoryEntry = new CompressUtils.CompressEntry();
                repositoryEntry.Name = NoteRepositoryModel.RepositoryFileName;
                repositoryEntry.Data = repositoryService.LoadRepositoryFile();

                byte[] zipArchiveContent = CompressUtils.CreateZipArchive(new[] { repositoryEntry });
                await folderPickerService.TrySaveFileToPickedFolder(
                    GetBackupFileName(), zipArchiveContent);
            }
        }

        /// <summary>
        /// Lets the user pick a backup file and loads this backup as the current repository.
        /// </summary>
        /// <param name="filePickerService">Service which can let the user pick a backup file.</param>
        /// <param name="repositoryService">Service which can store the repository.</param>
        /// <returns>Returns false if the selected backup file was invalid, returns true if the
        /// backup was loaded successfully or the user canceled loading of the backup.</returns>
        public static async Task<bool> TryRestoreBackup(IFilePickerService filePickerService, IRepositoryStorageService repositoryService)
        {
            if (await filePickerService.PickFile())
            {
                try
                {
                    byte[] fileContent = await filePickerService.ReadPickedFile();
                    var repositoryEntries = CompressUtils.OpenZipArchive(fileContent);
                    var repositoryEntry = repositoryEntries.Find(item => NoteRepositoryModel.RepositoryFileName.Equals(item.Name, StringComparison.InvariantCultureIgnoreCase));

                    if (repositoryEntry != null &&
                        repositoryService.TryLoadRepositoryFromFile(repositoryEntry.Data, out NoteRepositoryModel noteRepository))
                    {
                        repositoryService.TrySaveRepository(noteRepository);
                        return true;
                    }
                }
                catch (Exception)
                {
                }
                return false;
            }
            return true;
        }

        private static string GetBackupFileName()
        {
            string datePart = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            return NoteRepositoryModel.BackupFileMask.Replace("*", datePart);
        }
    }
}
