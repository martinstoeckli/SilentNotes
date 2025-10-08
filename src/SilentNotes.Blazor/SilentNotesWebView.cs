using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace SilentNotes
{
    public class SilentNotesWebView : BlazorWebView
    {
        public override IFileProvider CreateFileProvider(string contentRootDir)
        {
            IFileProvider originalFileProvider = base.CreateFileProvider(contentRootDir);
            IFileProvider attachementFileProvider = new AttachementFileProvider(originalFileProvider);
            CompositeFileProvider compositeFileProvider = new CompositeFileProvider(attachementFileProvider, originalFileProvider);
            return compositeFileProvider;
            //return new InterceptableFileProvider(originalFileProvider);
        }

        internal class AttachementFileProvider : IFileProvider
        {
            private readonly IFileProvider _fileProvider;

            public AttachementFileProvider(IFileProvider fileProvider)
            {
                _fileProvider = fileProvider;
            }

            public IDirectoryContents GetDirectoryContents(string subpath)
            {
                return null;
            }

            public IFileInfo GetFileInfo(string subpath)
            {
                Debug.WriteLine("stom: " + subpath);
                if (subpath.Contains("silentnoteimage/"))
                {
                    IFileInfo originalFileInfo = _fileProvider.GetFileInfo(subpath);
                    return new InterceptableFileInfo(originalFileInfo);
                }
                return null;
            }

            public IChangeToken Watch(string filter)
            {
                return null;
            }
        }

        private class InterceptableFileProvider : IFileProvider
        {
            private readonly IFileProvider _fileProvider;

            public InterceptableFileProvider(IFileProvider fileProvider   )
            {
                _fileProvider = fileProvider;
            }

            public IDirectoryContents GetDirectoryContents(string subpath)
            {
                return _fileProvider.GetDirectoryContents(subpath);
            }

            public IFileInfo GetFileInfo(string subpath)
            {
                IFileInfo originalFileInfo = _fileProvider.GetFileInfo(subpath);
                return new InterceptableFileInfo(originalFileInfo);
            }

            public IChangeToken Watch(string filter)
            {
                return _fileProvider.Watch(filter);
            }
        }

        public class InterceptableFileInfo : IFileInfo
        {
            private readonly IFileInfo _fileInfo;

            public InterceptableFileInfo(IFileInfo fileInfo)
            {
                _fileInfo = fileInfo;
            }

            public bool Exists => _fileInfo.Exists;
            public long Length => _fileInfo.Length;
            public string PhysicalPath => _fileInfo.PhysicalPath;
            public string Name => _fileInfo.Name;
            public DateTimeOffset LastModified => _fileInfo.LastModified;
            public bool IsDirectory => _fileInfo.IsDirectory;

            public Stream CreateReadStream()
            {
                return _fileInfo.CreateReadStream();
            }
        }
    }
}
