﻿
CREATE TABLE [dbo].[Channel](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nchar](20) NOT NULL,
	[Created] [datetime] NOT NULL,
	[IsPublic] [bit] NOT NULL,
 CONSTRAINT [PK_Channel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]


CREATE TABLE [dbo].[ChannelItem](
	[ChannelId] [bigint] NOT NULL,
	[PostId] [bigint] NOT NULL,
	[UserId] [bigint] NULL,
	[Created] [datetime] NULL,
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
	[Created] [datetime] NOT NULL,
	[ReplyingTo] [bigint] NULL,
 CONSTRAINT [PK_Comment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


CREATE TABLE [dbo].[FeederTags](
	[RssFeederId] [bigint] NOT NULL,
	[Tag] [nchar](255) NOT NULL,
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
	[Created] [datetime] NOT NULL,
	[Title] [nchar](255) NOT NULL,
	[Description] [text] NOT NULL,
	[Uri] [nchar](255) NOT NULL,
 CONSTRAINT [PK_Post] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


CREATE TABLE [dbo].[ReadHistory](
	[UserId] [bigint] NOT NULL,
	[PostId] [bigint] NOT NULL,
	[Timestamp] [datetime] NOT NULL,
 CONSTRAINT [PK_ReadHistory] PRIMARY KEY CLUSTERED 
(
	PostId ASC, UserId ASC
)
) ON [PRIMARY]


CREATE TABLE [dbo].[RssFeeder](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Url] [nchar](255) NOT NULL,
 CONSTRAINT [PK_RssFeeder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]


CREATE TABLE [dbo].[Subscription](
	[ChannelId] [bigint] NOT NULL,
	[UserId] [bigint] NOT NULL,
 CONSTRAINT [PK_Subscription] PRIMARY KEY CLUSTERED 
(
	[ChannelId] ASC, UserId ASC
)
) ON [PRIMARY]


CREATE TABLE [dbo].[Tagged](
	[PostId] [bigint] NOT NULL,
	[Tag] [nchar](64) NOT NULL,
 CONSTRAINT [PK_Tagged] PRIMARY KEY CLUSTERED 
(
	PostId ASC, Tag ASC
)
) ON [PRIMARY]


CREATE TABLE [dbo].[User](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Username] [nvarchar](50) NOT NULL,
	[SavedChannel] [bigint] NOT NULL,
	[StarredChannel] [bigint] NOT NULL,
	[LikedChannel] [bigint] NOT NULL,
	[IdentityId] [nchar](64) NULL,
	[Inbox] [bigint] NOT NULL,
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]


ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF_User_Inbox]  DEFAULT ((0)) FOR [Inbox]


CREATE TABLE [dbo].[UserLogins](
	[UserId] [bigint] NOT NULL,
	[ProviderKey] [nchar](64) NOT NULL,
	[LoginProvider] [nchar](64) NOT NULL,
 CONSTRAINT [PK_UserLogins] PRIMARY KEY CLUSTERED 
(
	UserId ASC, LoginProvider ASC
)
) ON [PRIMARY]


CREATE VIEW [dbo].[Popularity] as
select xx.PostId, COUNT(*) as Popularity from 
(select Post.Id as PostId
from Post  join ChannelItem ci on ci.PostId = Post.Id join Subscription s on s.ChannelId = ci.ChannelId
union all
select Post.Id as PostId 
from Post join ChannelItem ci on ci.PostId = Post.Id join ChannelOwner co on co.ChannelId = ci.ChannelId) as xx
group by PostId


CREATE VIEW [dbo].[UserSpecificPopularity]
AS
SELECT     PostId, UserId, COUNT(*) AS Popularity
FROM         (SELECT     dbo.Post.Id AS PostId, s.UserId
                       FROM          dbo.Post INNER JOIN
                                              dbo.ChannelItem AS ci ON ci.PostId = dbo.Post.Id INNER JOIN
                                              dbo.Subscription AS s ON s.ChannelId = ci.ChannelId
                       UNION ALL
                       SELECT     Post_1.Id AS PostId, co.UserId
                       FROM         dbo.Post AS Post_1 INNER JOIN
                                             dbo.ChannelItem AS ci ON ci.PostId = Post_1.Id INNER JOIN
                                             dbo.ChannelOwner AS co ON co.ChannelId = ci.ChannelId) AS xx
GROUP BY PostId, UserId