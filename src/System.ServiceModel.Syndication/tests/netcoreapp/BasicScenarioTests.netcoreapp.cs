﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel.Syndication;
using System.Xml;
using System.IO;
using Xunit;
using System.Linq;

namespace System.ServiceModel.Syndication.Tests
{
    public static partial class BasicScenarioTests
    {
        [Fact]
        public static void SyndicationFeed_Rss_DateTimeParser()
        {
            // *** SETUP *** \\
            // *** EXECUTE *** \\
            SyndicationFeed feed;
            DateTimeOffset dto = new DateTimeOffset(2017, 1, 2, 3, 4, 5, new TimeSpan(0));
            using (XmlReader reader = XmlReader.Create(@"RssSpecCustomParser.xml"))
            {
                var formatter = new Rss20FeedFormatter();
                formatter.DateTimeParser = (XmlDateTimeData xmlDateTimeData, out DateTimeOffset dateTimeOffset) =>
                {
                    dateTimeOffset = dto;
                    return true;
                };
                formatter.ReadFrom(reader);
                feed = formatter.Feed;
            }

            // *** ASSERT *** \\
            Assert.True(feed != null, "res was null.");
            Assert.Equal(dto, feed.LastUpdatedTime);
        }

        [Fact]
        public static void SyndicationFeed_Rss_UriParser()
        {
            // *** SETUP *** \\
            // *** EXECUTE *** \\
            SyndicationFeed feed;
            using (XmlReader reader = XmlReader.Create(@"RssSpecCustomParser.xml"))
            {
                var formatter = new Rss20FeedFormatter
                {
                    UriParser = (XmlUriData xmlUriData, out Uri uri) =>
                    {
                        uri = new Uri($"http://value-{xmlUriData.UriString}-kind-{xmlUriData.UriKind}-localName-{xmlUriData.ElementQualifiedName.Name}-ns-{xmlUriData.ElementQualifiedName.Namespace}-end");
                        return true;
                    }
                };
                formatter.ReadFrom(reader);
                feed = formatter.Feed;
            }

            // *** ASSERT *** \\
            Assert.True(feed != null, "res was null.");
            Assert.Equal(new Uri("http://value-ChannelBase-kind-relativeorabsolute-localName-channel-ns--end"), feed.BaseUri);
            Assert.Equal(new Uri("http://value-ImageUrl-kind-relativeorabsolute-localName-url-ns--end"), feed.ImageUrl);
            Assert.NotNull(feed.Links);
            Assert.Equal(1, feed.Links.Count);
            Assert.Equal(new Uri("http://value-FeedLink-kind-relativeorabsolute-localName-link-ns--end"), feed.Links.First().Uri);

            Assert.True(feed.Items != null, "res.Items was null.");
            Assert.Equal(1, feed.Items.Count());
            Assert.Equal(1, feed.Items.First().Links.Count);
            Assert.Equal(new Uri("http://value-itemlink-kind-relativeorabsolute-localName-link-ns--end"), feed.Items.First().Links.First().Uri);
        }

        [Fact]
        public static void SyndicationFeed_Atom_DateTimeParser()
        {
            // *** SETUP *** \\
            // *** EXECUTE *** \\
            SyndicationFeed feed;
            DateTimeOffset dto = new DateTimeOffset(2017, 1, 2, 3, 4, 5, new TimeSpan(0));
            using (XmlReader reader = XmlReader.Create(@"SimpleAtomFeedCustomParser.xml"))
            {
                var formatter = new Atom10FeedFormatter
                {
                    DateTimeParser = (XmlDateTimeData xmlDateTimeData, out DateTimeOffset dateTimeOffset) =>
                    {
                        dateTimeOffset = dto;
                        return true;
                    }
                };
                formatter.ReadFrom(reader);
                feed = formatter.Feed;
            }

            // *** ASSERT *** \\
            Assert.True(feed != null, "res was null.");
            Assert.Equal(dto, feed.LastUpdatedTime);

            Assert.True(feed.Items != null, "res.Items was null.");
            Assert.Equal(1, feed.Items.Count());
            Assert.Equal(dto, feed.Items.First().LastUpdatedTime);
        }

        [Fact]
        public static void SyndicationFeed_Atom_UriParser()
        {
            // *** SETUP *** \\
            // *** EXECUTE *** \\
            SyndicationFeed feed;
            using (XmlReader reader = XmlReader.Create(@"SimpleAtomFeedCustomParser.xml"))
            {
                var formatter = new Atom10FeedFormatter
                {
                    UriParser = (XmlUriData xmlUriData, out Uri uri) =>
                    {
                        uri = new Uri($"http://value-{xmlUriData.UriString}-kind-{xmlUriData.UriKind}-localName-{xmlUriData.ElementQualifiedName.Name}-ns-{xmlUriData.ElementQualifiedName.Namespace}-end");
                        return true;
                    }
                };
                formatter.ReadFrom(reader);
                feed = formatter.Feed;
            }

            // *** ASSERT *** \\
            Assert.True(feed != null, "res was null.");
            Assert.Equal(new Uri("http://value-FeedLogo-kind-relativeorabsolute-localName-logo-ns-http//www.w3.org/2005/Atom-end"), feed.ImageUrl);

            Assert.True(feed.Items != null, "res.Items was null.");
            Assert.Equal(1, feed.Items.Count());
            Assert.NotNull(feed.Items.First().Links);
            Assert.Equal(1, feed.Items.First().Links.Count);
            Assert.Equal(new Uri("http://value-EntryLinkHref-kind-relativeorabsolute-localName-link-ns-http//www.w3.org/2005/Atom-end"), feed.Items.First().Links.First().Uri);
            Assert.Equal(new Uri("http://value-EntryContentSrc-kind-relativeorabsolute-localName-content-ns-http://www.w3.org/2005/Atom-end"), ((UrlSyndicationContent)feed.Items.First().Content).Url);
        }
        
        [Fact]
        public static void SyndicationFeed_RSS_Optional_Documentation()
        {
            using (XmlReader reader = XmlReader.Create(@"rssSpecExample.xml"))
            {
                SyndicationFeed rss = SyndicationFeed.Load(reader);
                Assert.NotNull(rss.Documentation);
                Assert.True(rss.Documentation.GetAbsoluteUri().ToString() == "http://contoso.com/rss");
            }
        }

        [Fact]
        public static void SyndicationFeed_Load_Write_RSS_Documentation()
        {
            List<AllowableDifference> allowableDifferences = GetRssFeedPositiveTestAllowableDifferences();
            ReadWriteSyndicationFeed(
                file: "rssOptionalElements.xml",
                feedFormatter: (feedObject) => new Rss20FeedFormatter(feedObject),
                allowableDifferences: allowableDifferences,
                verifySyndicationFeedRead: (feed) =>
                {
                    Assert.NotNull(feed);
                    Assert.NotNull(feed.Documentation);
                    Assert.True(feed.Documentation.GetAbsoluteUri().ToString() == "http://contoso.com/rss");
                });
        }
    }
}
