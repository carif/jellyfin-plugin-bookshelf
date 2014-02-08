﻿using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Controller.Providers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security;
using System.Text;
using System.Threading;
using MediaBrowser.Model.Entities;

namespace MediaBrowser.Plugins.XbmcMetadata.Savers
{
    public class EpisodeXmlSaver : IMetadataFileSaver
    {
        private readonly ILibraryManager _libraryManager;
        private readonly IUserManager _userManager;
        private readonly IUserDataManager _userDataRepo;
        private readonly IItemRepository _itemRepo;

        private readonly CultureInfo _usCulture = new CultureInfo("en-US");

        public EpisodeXmlSaver(ILibraryManager libraryManager, IUserManager userManager, IUserDataManager userDataRepo, IItemRepository itemRepo)
        {
            _libraryManager = libraryManager;
            _userManager = userManager;
            _userDataRepo = userDataRepo;
            _itemRepo = itemRepo;
        }

        public string Name
        {
            get
            {
                return "Xbmc nfo";
            }
        }

        public string GetSavePath(IHasMetadata item)
        {
            return Path.ChangeExtension(item.Path, ".nfo");
        }

        public void Save(IHasMetadata item, CancellationToken cancellationToken)
        {
            var episode = (Episode)item;

            var builder = new StringBuilder();

            builder.Append("<episodedetails>");

            XmlSaverHelpers.AddCommonNodes(episode, builder, _libraryManager, _userManager, _userDataRepo);

            if (episode.IndexNumber.HasValue)
            {
                builder.Append("<episode>" + episode.IndexNumber.Value.ToString(_usCulture) + "</episode>");
            }

            if (episode.ParentIndexNumber.HasValue)
            {
                builder.Append("<season>" + episode.ParentIndexNumber.Value.ToString(_usCulture) + "</season>");
            }

            if (episode.PremiereDate.HasValue)
            {
                var formatString = Plugin.Instance.Configuration.ReleaseDateFormat;

                builder.Append("<aired>" + SecurityElement.Escape(episode.PremiereDate.Value.ToString(formatString)) + "</aired>");
            }

            if (episode.AirsAfterSeasonNumber.HasValue)
            {
                builder.Append("<airsafter_season>" + SecurityElement.Escape(episode.AirsAfterSeasonNumber.Value.ToString(_usCulture)) + "</airsafter_season>");
            }
            if (episode.AirsBeforeEpisodeNumber.HasValue)
            {
                builder.Append("<airsbefore_episode>" + SecurityElement.Escape(episode.AirsBeforeEpisodeNumber.Value.ToString(_usCulture)) + "</airsbefore_episode>");
            }
            if (episode.AirsBeforeSeasonNumber.HasValue)
            {
                builder.Append("<airsbefore_season>" + SecurityElement.Escape(episode.AirsBeforeSeasonNumber.Value.ToString(_usCulture)) + "</airsbefore_season>");
            }

            if (episode.DvdEpisodeNumber.HasValue)
            {
                builder.Append("<DVD_episodenumber>" + SecurityElement.Escape(episode.DvdEpisodeNumber.Value.ToString(_usCulture)) + "</DVD_episodenumber>");
            }

            if (episode.DvdSeasonNumber.HasValue)
            {
                builder.Append("<DVD_season>" + SecurityElement.Escape(episode.DvdSeasonNumber.Value.ToString(_usCulture)) + "</DVD_season>");
            }

            if (episode.AbsoluteEpisodeNumber.HasValue)
            {
                builder.Append("<absolute_number>" + SecurityElement.Escape(episode.AbsoluteEpisodeNumber.Value.ToString(_usCulture)) + "</absolute_number>");
            }
            
            XmlSaverHelpers.AddMediaInfo((Episode)item, _itemRepo, builder);

            builder.Append("</episodedetails>");

            var xmlFilePath = GetSavePath(item);

            XmlSaverHelpers.Save(builder, xmlFilePath, new List<string>
                {
                    "aired",
                    "season",
                    "episode",
                    "airsafter_season",
                    "airsbefore_episode",
                    "airsbefore_season",
                    "DVD_episodenumber",
                    "DVD_season",
                    "absolute_number"
                });
        }

        public bool IsEnabledFor(IHasMetadata item, ItemUpdateType updateType)
        {
            var locationType = item.LocationType;
            if (locationType == LocationType.Remote || locationType == LocationType.Virtual)
            {
                return false;
            }

            // If new metadata has been downloaded or metadata was manually edited, proceed
            if ((updateType & ItemUpdateType.MetadataDownload) == ItemUpdateType.MetadataDownload
                || (updateType & ItemUpdateType.MetadataEdit) == ItemUpdateType.MetadataEdit)
            {
                return item is Episode;
            }

            return false;
        }
    }
}
