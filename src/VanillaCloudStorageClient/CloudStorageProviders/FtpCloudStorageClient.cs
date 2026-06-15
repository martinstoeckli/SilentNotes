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
        private const int DefaultTimeoutSeconds = 15;
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
        public override async Task UploadFileAsync(string filename, byte[] fileContent, CloudStorageCredentials credentials)
        {
            credentials.ThrowIfInvalid(CredentialsRequirements, true);

            try
            {
                Url fileUrl = new Url(credentials.Url).AppendPathSegment(filename);
                using (var ftp = new AsyncFtpClient(fileUrl.Host, new NetworkCredential(credentials.Username, credentials.Password)))
                {
                    ConfigureFtpClient(ftp, credentials, UploadTimeoutSeconds * 1000);
                    if (!IsInTestMode)
                    {
                        _lastConnectionProfile = await ConnectOrAutoConnect(ftp, _lastConnectionProfile);
                        await ftp.UploadBytes(fileContent, fileUrl.Path);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ConvertToCloudStorageException(ex);
            }
        }

        /// <inheritdoc/>
        public override async Task<byte[]> DownloadFileAsync(string filename, CloudStorageCredentials credentials)
        {
            credentials.ThrowIfInvalid(CredentialsRequirements, true);

            try
            {
                Url fileUrl = new Url(credentials.Url).AppendPathSegment(filename);
                byte[] responseData;
                using (var ftp = new AsyncFtpClient(fileUrl.Host, new NetworkCredential(credentials.Username, credentials.Password)))
                {
                    ConfigureFtpClient(ftp, credentials, DownloadTimeoutSeconds * 1000);
                    if (IsInTestMode)
                    {
                        responseData = _fakeResponse.GetFakeServerResponseBytes(new Url(credentials.Url).AppendPathSegment(filename));
                    }
                    else
                    {
                        _lastConnectionProfile = await ConnectOrAutoConnect(ftp, _lastConnectionProfile);
                        responseData = await ftp.DownloadBytes(fileUrl.Path, 0);
                    }
                }
                return responseData;
            }
            catch (Exception ex)
            {
                throw ConvertToCloudStorageException(ex);
            }
        }

        /// <inheritdoc/>
        public override async Task DeleteFileAsync(string filename, CloudStorageCredentials credentials)
        {
            credentials.ThrowIfInvalid(CredentialsRequirements, true);

            try
            {
                Url fileUrl = new Url(credentials.Url).AppendPathSegment(filename);
                using (var ftp = new AsyncFtpClient(fileUrl.Host, new NetworkCredential(credentials.Username, credentials.Password)))
                {
                    ConfigureFtpClient(ftp, credentials);
                    if (!IsInTestMode)
                    {
                        _lastConnectionProfile = await ConnectOrAutoConnect(ftp, _lastConnectionProfile);
                        await ftp.DeleteFile(fileUrl.Path);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ConvertToCloudStorageException(ex);
            }
        }

        public override async Task<bool> ExistsFileAsync(string filename, CloudStorageCredentials credentials)
        {
            credentials.ThrowIfInvalid(CredentialsRequirements, true);

            try
            {
                Url fileUrl = new Url(credentials.Url).AppendPathSegment(filename);
                bool result;
                using (var ftp = new AsyncFtpClient(fileUrl.Host, new NetworkCredential(credentials.Username, credentials.Password)))
                {
                    ConfigureFtpClient(ftp, credentials);
                    if (IsInTestMode)
                    {
                        result = _fakeResponse.GetFakeServerExistsFile(fileUrl.Path);
                    }
                    else
                    {
                        _lastConnectionProfile = await ConnectOrAutoConnect(ftp, _lastConnectionProfile);
                        result = await ftp.FileExists(fileUrl.Path);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ConvertToCloudStorageException(ex);
            }
        }

        /// <inheritdoc/>
        public override async Task<List<string>> ListFileNamesAsync(CloudStorageCredentials credentials)
        {
            credentials.ThrowIfInvalid(CredentialsRequirements, true);

            try
            {
                Url directoryUrl = new Url(credentials.Url);
                string[] fileNames = null;
                using (var ftp = new AsyncFtpClient(directoryUrl.Host, new NetworkCredential(credentials.Username, credentials.Password)))
                {
                    ConfigureFtpClient(ftp, credentials);
                    if (IsInTestMode)
                    {
                        string responseData = _fakeResponse.GetFakeServerResponseString(credentials.Url);

                        // Interpret the response
                        string unixDelimitedResponse = responseData.Replace("\r\n", "\n");
                        fileNames = unixDelimitedResponse.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    else
                    {
                        _lastConnectionProfile = await ConnectOrAutoConnect(ftp, _lastConnectionProfile);
                        fileNames = await ftp.GetNameListing(directoryUrl.Path?.TrimEnd('/'));
                    }
                }

                List<string> result = fileNames.Select(fileName => Path.GetFileName(fileName)).ToList();
                result.Remove("..");
                result.Remove(".");
                return result;
            }
            catch (Exception ex)
            {
                throw ConvertToCloudStorageException(ex);
            }
        }

        private static async Task<FtpProfile> ConnectOrAutoConnect(AsyncFtpClient ftpClient, FtpProfile lastProfile)
        {
            bool canReuseLastProfile = (lastProfile != null) && (string.Equals(ftpClient.Host, lastProfile.Host));
            if (canReuseLastProfile)
            {
                await ftpClient.Connect(lastProfile);
                return lastProfile;
            }
            else
            {
                FtpProfile connectionProfile = await ftpClient.AutoConnect();
                if (connectionProfile == null)
                    throw new ConnectionFailedException();
                return connectionProfile;
            }
        }

        private static void ConfigureFtpClient(AsyncFtpClient ftp, CloudStorageCredentials credentials, int timeoutSecondsMs = DefaultTimeoutSeconds * 1000)
        {
            ftp.Config.ValidateAnyCertificate = credentials.AcceptInvalidCertificate;
            ftp.Config.EncryptionMode = credentials.Secure ? FtpEncryptionMode.Explicit : FtpEncryptionMode.Auto;
            ftp.Config.DataConnectionEncryption = credentials.Secure;
            ftp.Config.ReadTimeout = timeoutSecondsMs;
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
