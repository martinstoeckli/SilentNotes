// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentFTP;
using Flurl;

namespace VanillaCloudStorageClient.CloudStorageProviders
{
    /// <summary>
    /// Implementation of the <see cref="ICloudStorageClient"/> interface,
    /// which can handle cloud storage on FTP servers.
    /// </summary>
    public class FtpCloudStorageClient : CloudStorageClientBase, ICloudStorageClient
    {
        private const int UploadTimeoutSeconds = 40;
        private const int DownloadTimeoutSeconds = 30;
        private readonly IFtpFakeResponse _fakeResponse;
        private FtpProfile _lastConnectionProfile;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCloudStorageClient"/> class.
        /// </summary>
        public FtpCloudStorageClient()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCloudStorageClient"/> class.
        /// </summary>
        /// <param name="fakeResponse">If this parameter is not null, the storage client is put
        /// into a test mode and no real calls to the Ftp server are made. Instead a Mock can
        /// define fake responses.</param>
        public FtpCloudStorageClient(IFtpFakeResponse fakeResponse)
        {
            _fakeResponse = fakeResponse;
        }

        /// <inheritdoc/>
        public override CloudStorageCredentialsRequirements CredentialsRequirements
        {
            get { return CloudStorageCredentialsRequirements.Username | CloudStorageCredentialsRequirements.Password | CloudStorageCredentialsRequirements.Url | CloudStorageCredentialsRequirements.Secure | CloudStorageCredentialsRequirements.AcceptUnsafeCertificate; }
        }

        /// <inheritdoc/>
        public override Task UploadFileAsync(string filename, byte[] fileContent, CloudStorageCredentials credentials)
        {
            credentials.ThrowIfInvalid(CredentialsRequirements, true);

            try
            {
                Url fileUrl = new Url(credentials.Url).AppendPathSegment(filename);
                using (var ftp = new FtpClient(fileUrl.Host, new NetworkCredential(credentials.Username, credentials.Password)))
                {
                    ftp.Config.ValidateAnyCertificate = credentials.AcceptInvalidCertificate;
                    ftp.Config.ReadTimeout = UploadTimeoutSeconds * 1000;
                    if (!IsInTestMode)
                    {
                        _lastConnectionProfile = ConnectOrAutoConnect(ftp, _lastConnectionProfile);
                        ftp.UploadBytes(fileContent, fileUrl.Path);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ConvertToCloudStorageException(ex);
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override Task<byte[]> DownloadFileAsync(string filename, CloudStorageCredentials credentials)
        {
            credentials.ThrowIfInvalid(CredentialsRequirements, true);

            try
            {
                Url fileUrl = new Url(credentials.Url).AppendPathSegment(filename);
                byte[] responseData;
                using (var ftp = new FtpClient(fileUrl.Host, new NetworkCredential(credentials.Username, credentials.Password)))
                {
                    ftp.Config.ValidateAnyCertificate = credentials.AcceptInvalidCertificate;
                    ftp.Config.ReadTimeout = DownloadTimeoutSeconds * 1000;
                    if (IsInTestMode)
                    {
                        responseData = _fakeResponse.GetFakeServerResponseBytes(new Url(credentials.Url).AppendPathSegment(filename));
                    }
                    else
                    {
                        _lastConnectionProfile = ConnectOrAutoConnect(ftp, _lastConnectionProfile);
                        ftp.DownloadBytes(out responseData, fileUrl.Path, 0);
                    }
                }
                return Task.FromResult(responseData);
            }
            catch (Exception ex)
            {
                throw ConvertToCloudStorageException(ex);
            }
        }

        /// <inheritdoc/>
        public override Task DeleteFileAsync(string filename, CloudStorageCredentials credentials)
        {
            credentials.ThrowIfInvalid(CredentialsRequirements, true);

            try
            {
                Url fileUrl = new Url(credentials.Url).AppendPathSegment(filename);
                using (var ftp = new FtpClient(fileUrl.Host, new NetworkCredential(credentials.Username, credentials.Password)))
                {
                    ftp.Config.ValidateAnyCertificate = credentials.AcceptInvalidCertificate;
                    if (!IsInTestMode)
                    {
                        _lastConnectionProfile = ConnectOrAutoConnect(ftp, _lastConnectionProfile);
                        ftp.DeleteFile(fileUrl.Path);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ConvertToCloudStorageException(ex);
            }
            return Task.CompletedTask;
        }

        public override Task<bool> ExistsFileAsync(string filename, CloudStorageCredentials credentials)
        {
            credentials.ThrowIfInvalid(CredentialsRequirements, true);

            try
            {
                Url fileUrl = new Url(credentials.Url).AppendPathSegment(filename);
                bool result;
                using (var ftp = new FtpClient(fileUrl.Host, new NetworkCredential(credentials.Username, credentials.Password)))
                {
                    ftp.Config.ValidateAnyCertificate = credentials.AcceptInvalidCertificate;
                    if (IsInTestMode)
                    {
                        result = _fakeResponse.GetFakeServerExistsFile(credentials.Url);
                    }
                    else
                    {
                        _lastConnectionProfile = ConnectOrAutoConnect(ftp, _lastConnectionProfile);
                        result = ftp.FileExists(fileUrl.Path);
                    }
                }

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                throw ConvertToCloudStorageException(ex);
            }
        }

        /// <inheritdoc/>
        public override Task<List<string>> ListFileNamesAsync(CloudStorageCredentials credentials)
        {
            credentials.ThrowIfInvalid(CredentialsRequirements, true);

            try
            {
                Url directoryUrl = new Url(credentials.Url);
                string[] fileNames = null;
                using (var ftp = new FtpClient(directoryUrl.Host, new NetworkCredential(credentials.Username, credentials.Password)))
                {
                    ftp.Config.ValidateAnyCertificate = credentials.AcceptInvalidCertificate;
                    if (IsInTestMode)
                    {
                        string responseData = _fakeResponse.GetFakeServerResponseString(credentials.Url);

                        // Interpret the response
                        string unixDelimitedResponse = responseData.Replace("\r\n", "\n");
                        fileNames = unixDelimitedResponse.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    else
                    {
                        _lastConnectionProfile = ConnectOrAutoConnect(ftp, _lastConnectionProfile);
                        fileNames = ftp.GetNameListing(directoryUrl.Path?.TrimEnd('/'));
                    }
                }

                List<string> result = fileNames.Select(fileName => Path.GetFileName(fileName)).ToList();
                result.Remove("..");
                result.Remove(".");
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                throw ConvertToCloudStorageException(ex);
            }
        }

        private static FtpProfile ConnectOrAutoConnect(FtpClient ftpClient, FtpProfile lastProfile)
        {
            bool canReuseLastProfile = (lastProfile != null) && (string.Equals(ftpClient.Host, lastProfile.Host));
            if (canReuseLastProfile)
            {
                ftpClient.Connect(lastProfile);
                return lastProfile;
            }
            else
            {
                FtpProfile connectionProfile = ftpClient.AutoConnect();
                if (connectionProfile == null)
                    throw new ConnectionFailedException();
                return connectionProfile;
            }
        }

        private bool IsInTestMode
        {
            get { return _fakeResponse != null; }
        }
    }

    /// <summary>
    /// This interface can be mocked and passed to the <see cref="FtpCloudStorageClient"/> for unittesting.
    /// </summary>
    public interface IFtpFakeResponse
    {
        /// <summary>
        /// This method can be mocked to provide faked server responses.
        /// </summary>
        /// <param name="url">The url called by the <see cref="FtpCloudStorageClient"/>.</param>
        /// <returns>The mock returns the fake response.</returns>
        string GetFakeServerResponseString(string url);

        /// <summary>
        /// This method can be mocked to provide faked server responses.
        /// </summary>
        /// <param name="url">The url called by the <see cref="FtpCloudStorageClient"/>.</param>
        /// <returns>The mock returns the fake response.</returns>
        byte[] GetFakeServerResponseBytes(string url);

        /// <summary>
        /// T
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        bool GetFakeServerExistsFile(string url);
    }
}
