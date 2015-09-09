
CREATE TABLE [dbo].[Channel](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nchar](20) NOT NULL,
	[Created] [datetime] NOT NULL,
	[IsPublic] [bit] NOT NULL
) ON [PRIMARY]


CREATE TABLE [dbo].[ChannelItem](
	[ChannelId] [bigint] NOT NULL,
	[PostId] [bigint] NOT NULL,
	[UserId] [bigint] NULL,
	[Created] [datetime] NULL
) ON [PRIMARY]


CREATE TABLE [dbo].[ChannelOwner](
	[ChannelId] [bigint] NOT NULL,
	[UserId] [bigint] NOT NULL,
	[IsLocked] [bit] NOT NULL
) ON [PRIMARY]


CREATE TABLE [dbo].[Comment](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [bigint] NOT NULL,
	[PostId] [bigint] NOT NULL,
	[Text] [text] NOT NULL,
	[Created] [datetime] NOT NULL,
	[ReplyingTo] [bigint] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


CREATE TABLE [dbo].[FeederTags](
	[RssFeederId] [bigint] NOT NULL,
	[Tag] [nchar](255) NOT NULL
) ON [PRIMARY]


CREATE TABLE [dbo].[FeedInto](
	[RssFeederId] [bigint] NOT NULL,
	[ChannelId] [bigint] NOT NULL
) ON [PRIMARY]


CREATE TABLE [dbo].[Post](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Created] [datetime] NOT NULL,
	[Title] [nchar](255) NOT NULL,
	[Description] [text] NOT NULL,
	[Uri] [nchar](255) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


CREATE TABLE [dbo].[ReadHistory](
	[UserId] [bigint] NOT NULL,
	[PostId] [bigint] NOT NULL,
	[Timestamp] [datetime] NOT NULL
) ON [PRIMARY]


CREATE TABLE [dbo].[RssFeeder](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Url] [nchar](255) NOT NULL
) ON [PRIMARY]


CREATE TABLE [dbo].[Subscription](
	[ChannelId] [bigint] NOT NULL,
	[UserId] [bigint] NOT NULL
) ON [PRIMARY]


CREATE TABLE [dbo].[Tagged](
	[PostId] [bigint] NOT NULL,
	[Tag] [nchar](64) NOT NULL
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
	[LoginProvider] [nchar](64) NOT NULL
) ON [PRIMARY]