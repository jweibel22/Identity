﻿
CREATE TABLE [dbo].[Channel](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[Created] [datetimeoffset] NOT NULL,
	[IsPublic] [bit] NOT NULL
 CONSTRAINT [PK_Channel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]


CREATE TABLE [dbo].[ChannelItem](
	[ChannelId] [bigint] NOT NULL,
	[PostId] [bigint] NOT NULL,
	[UserId] [bigint] NULL,
	[Created] [datetimeoffset] NULL,
 CONSTRAINT [PK_ChannelItem] PRIMARY KEY CLUSTERED 
(
	[ChannelId] ASC, PostId ASC
)
) ON [PRIMARY]


CREATE TABLE [dbo].[ChannelOwner](
	[ChannelId] [bigint] NOT NULL,
	[UserId] [bigint] NOT NULL,
	[IsLocked] [bit] NOT NULL,
 CONSTRAINT [PK_ChannelOwner] PRIMARY KEY CLUSTERED 
(
	[ChannelId] ASC, UserId ASC
)
) ON [PRIMARY]

CREATE TABLE [dbo].[Comment](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [bigint] NOT NULL,
	[PostId] [bigint] NOT NULL,
	[Text] [text] NOT NULL,
	[Created] [datetimeoffset] NOT NULL,
	[ReplyingTo] [bigint] NULL,
 CONSTRAINT [PK_Comment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


CREATE TABLE [dbo].[FeederTags](
	[RssFeederId] [bigint] NOT NULL,
	[Tag] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_FeederTags] PRIMARY KEY CLUSTERED 
(
	[RssFeederId] ASC
)
) ON [PRIMARY]


CREATE TABLE [dbo].[FeedInto](
	[RssFeederId] [bigint] NOT NULL,
	[ChannelId] [bigint] NOT NULL,
 CONSTRAINT [PK_FeedInto] PRIMARY KEY CLUSTERED 
(
	[ChannelId] ASC, RssFeederId ASC
)
) ON [PRIMARY]


CREATE TABLE [dbo].[Post](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Created] [datetimeoffset] NOT NULL,
	[Title] [nvarchar](255) NOT NULL,
	[Description] [text] NOT NULL,
	[Uri] [nvarchar](255) NOT NULL,
	[PremiumContent] [bit] NOT NULL,
 CONSTRAINT [PK_Post] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE UNIQUE NONCLUSTERED INDEX [Post_Uri_Index] ON [dbo].[Post]
(
	[Uri] ASC
)


CREATE TABLE [dbo].[ReadHistory](
	[UserId] [bigint] NOT NULL,
	[PostId] [bigint] NOT NULL,
	[Timestamp] [datetimeoffset] NOT NULL,
 CONSTRAINT [PK_ReadHistory] PRIMARY KEY CLUSTERED 
(
	PostId ASC, UserId ASC
)
) ON [PRIMARY]


CREATE TABLE [dbo].[RssFeeder](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Url] [nvarchar](255) NOT NULL,
	[LastFetch] [datetimeoffset] NULL,
	[Type] [int] NOT NULL,
	[ChannelId] bigint NOT NULL,
 CONSTRAINT [PK_RssFeeder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]


CREATE TABLE [dbo].[Tagged](
	[PostId] [bigint] NOT NULL,
	[TagId] [bigint] NOT NULL,
	[Confirmed] [bit] NOT NULL default 1,
 CONSTRAINT [PK_Tagged3] PRIMARY KEY CLUSTERED 
(
	PostId ASC, TagId ASC
)
) ON [PRIMARY]


CREATE NONCLUSTERED INDEX [Tagged_Tag_Index] ON [dbo].[Tagged]
(
	[Tag] ASC
)

CREATE TABLE [dbo].[User](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Username] [nvarchar](50) NOT NULL,
	[SavedChannel] [bigint] NOT NULL,
	[StarredChannel] [bigint] NOT NULL,
	[LikedChannel] [bigint] NOT NULL,
	[IdentityId] [nvarchar](64) NULL,
	[Inbox] [bigint] NOT NULL,
	[SubscriptionChannel] [bigint] NOT NULL,
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]


ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF_User_Inbox]  DEFAULT ((0)) FOR [Inbox]


CREATE TABLE [dbo].[UserLogins](
	[UserId] [bigint] NOT NULL,
	[ProviderKey] [nvarchar](64) NOT NULL,
	[LoginProvider] [nvarchar](64) NOT NULL,
 CONSTRAINT [PK_UserLogins] PRIMARY KEY CLUSTERED 
(
	UserId ASC, LoginProvider ASC
)
) ON [PRIMARY]


CREATE TABLE [dbo].[ChannelLink](
	[ParentId] [bigint] NOT NULL,
	[ChildId] [bigint] NOT NULL,
 CONSTRAINT [PK_ChannelLink] PRIMARY KEY CLUSTERED 
(
	[ParentId] ASC,
	[ChildId] ASC
)
) ON [PRIMARY]


CREATE TABLE [dbo].[FeedItem](
	[RssFeederId] [bigint] NOT NULL,
	[PostId] [bigint] NOT NULL,
	[Created] [datetimeoffset] NULL,
 CONSTRAINT [PK_FeedItem] PRIMARY KEY CLUSTERED 
(
	[RssFeederId] ASC, PostId ASC
)
) ON [PRIMARY]

CREATE TABLE [dbo].[ChannelTag](
	[ChannelId] [bigint] NOT NULL,
	[Tag] [nvarchar](64) NOT NULL,
	[Count] [bigint] NULL,
 CONSTRAINT [PK_ChannelTag] PRIMARY KEY CLUSTERED 
(
	[ChannelId] ASC, Tag ASC
)
) ON [PRIMARY];


CREATE VIEW [dbo].[Popularity] as
select xx.PostId, COUNT(*) as Popularity from 
(select Post.Id as PostId
from Post  join ChannelItem ci on ci.PostId = Post.Id join Subscription s on s.ChannelId = ci.ChannelId
union all
select Post.Id as PostId 
from Post join ChannelItem ci on ci.PostId = Post.Id join ChannelOwner co on co.ChannelId = ci.ChannelId) as xx
group by PostId;


CREATE TABLE [dbo].[ChannelDisplaySettings](
	[ChannelId] [bigint] NOT NULL,
	[UserId] [bigint] NOT NULL,
	[Settings] nvarchar(max),
 CONSTRAINT [PK_ChannelDisplaySettings] PRIMARY KEY CLUSTERED 
(
	[ChannelId] ASC,
	[UserId] ASC
),
CONSTRAINT [Content should be formatted as JSON]
 CHECK ( ISJSON(Settings)>0 )
) ON [PRIMARY]


CREATE TABLE [dbo].[InlineArticleSelector](
	[UrlPattern] [nvarchar](255) NOT NULL,
	[Selector] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_InlineArticleSelector] PRIMARY KEY CLUSTERED 
(
	[UrlPattern] ASC
)
) ON [PRIMARY]

CREATE TABLE [dbo].[Tag](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[ML_Enabled] [bit] NOT NULL default 0
 CONSTRAINT [PK_Tag] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]


CREATE TABLE [dbo].[Word](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Contents] [nvarchar](128) NOT NULL
 CONSTRAINT [PK_Word] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]


CREATE TABLE [dbo].[PostTitleWords](
	[WordId] [bigint] NOT NULL,
	[PostId] [bigint] NOT NULL,
	[Count] [int] NOT NULL default 1,
 CONSTRAINT [PK_PostTitleWords] PRIMARY KEY CLUSTERED 
(
	WordId ASC, PostId ASC
)
) ON [PRIMARY]

CREATE TABLE [dbo].[WebScraper](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Url] [nvarchar](255) NOT NULL,
	[LastFetch] [datetimeoffset] NULL,
	[Algorithm] int NOT NULL,
 CONSTRAINT [PK_WebScraper] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]

CREATE TABLE [dbo].[WebScraperItem](
	[WebScraperId] [bigint] NOT NULL,
	[PostId] [bigint] NOT NULL,
	[Created] [datetimeoffset] NULL,
 CONSTRAINT [PK_WebScraperItem] PRIMARY KEY CLUSTERED 
(
	[WebScraperId] ASC, PostId ASC
)
) ON [PRIMARY]

CREATE TABLE [dbo].[BlockedTag](
	[UserId] [bigint] NOT NULL,
	[TagId] [bigint] NOT NULL,
 CONSTRAINT [PK_BlockedTag] PRIMARY KEY CLUSTERED 
(
	UserId ASC, TagId ASC
)
) ON [PRIMARY]