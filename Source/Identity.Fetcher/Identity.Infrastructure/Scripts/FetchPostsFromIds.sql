create procedure [dbo].[FetchPostsFromIds]
	@UserId bigint,
	@PostIds id_list READONLY
as 
begin
select 
Post.*, 
0 as UserSpecificPopularity,
Post.Created as Added,
0 as Popularity,
CASE WHEN liked.Created IS NULL THEN 'false' ELSE 'true' END as Liked, 
CASE WHEN saved.Created IS NULL THEN 'false' ELSE 'true' END as Saved, 
CASE WHEN starred.Created IS NULL THEN 'false' ELSE 'true' END as Starred,
CASE WHEN ReadHistory.Timestamp IS NULL THEN 'false' ELSE 'true' END as [Read]
  FROM [dbo].[Post]
  join [User] u on u.Id = @UserId
  left join ReadHistory on ReadHistory.PostId = Post.Id and ReadHistory.UserId = @UserId
left join ChannelItem liked on liked.ChannelId = u.LikedChannel and liked.PostId = Post.Id
left join ChannelItem saved on saved.ChannelId = u.SavedChannel and saved.PostId = Post.Id
left join ChannelItem starred on starred.ChannelId = u.StarredChannel and starred.PostId = Post.Id
  where Post.Id in (select id from @PostIds)
  end