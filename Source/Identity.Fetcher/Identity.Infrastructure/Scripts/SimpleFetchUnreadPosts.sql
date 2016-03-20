create procedure SimpleFetchPosts
	@UserId bigint,
	@FromIndex int,
	@ChannelId bigint,
	@Timestamp Date,
	@PageSize int
as 
begin
select * from (select 
Post.*, 
0 as UserSpecificPopularity,
Post.Created as Added,
0 as Popularity,
ROW_NUMBER() OVER (ORDER BY Post.Created desc) AS RowNum,
CASE WHEN liked.Created IS NULL THEN 'false' ELSE 'true' END as Liked, 
CASE WHEN saved.Created IS NULL THEN 'false' ELSE 'true' END as Saved, 
CASE WHEN starred.Created IS NULL THEN 'false' ELSE 'true' END as Starred,
CASE WHEN ReadHistory.Timestamp IS NULL THEN 'false' ELSE 'true' END as [Read]
  FROM [dbo].[Post]
  left join ReadHistory on ReadHistory.PostId = Post.Id and ReadHistory.UserId = @UserId
join [User] u on u.Id = 4
inner join ChannelItem ci on ci.PostId = Post.Id
left join ChannelLink cl on cl.ChildId = ci.ChannelId
left join ChannelItem liked on liked.ChannelId = u.LikedChannel and liked.PostId = Post.Id
left join ChannelItem saved on saved.ChannelId = u.SavedChannel and saved.PostId = Post.Id
left join ChannelItem starred on starred.ChannelId = u.StarredChannel and starred.PostId = Post.Id
  where ReadHistory.Timestamp is null
  and (ci.ChannelId = @ChannelId or cl.ParentId = @ChannelId)
  ) as TBL
  where TBL.RowNum BETWEEN (@FromIndex+1) AND (@FromIndex+@PageSize)
  order by TBL.Created desc
  end
  --TODO: Handle the fact that the same post can be posted multiple times to the same Channel (currently this will result in duplicates in the output)