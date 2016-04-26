create procedure [dbo].[FetchUnreadPostsIds]
	@UserId bigint,
	@FromIndex int,
	@ChannelId bigint,
	@PageSize int
as 
begin

select Id from 
(select Id, ROW_NUMBER() OVER (ORDER BY AllPosts.Created desc) AS RowNum
 from (select Post.Id, Post.Created
  FROM [dbo].[Post]  
  left join ReadHistory on ReadHistory.PostId = Post.Id and ReadHistory.UserId = @UserId
inner join ChannelItem ci on ci.PostId = Post.Id
  where ReadHistory.Timestamp is null and ci.ChannelId = @ChannelId
  
  union

  select Post.Id, Post.Created
  FROM [dbo].[Post]  
  left join ReadHistory on ReadHistory.PostId = Post.Id and ReadHistory.UserId = @UserId
inner join ChannelItem ci on ci.PostId = Post.Id
left join ChannelLink cl on cl.ChildId = ci.ChannelId  
  where ReadHistory.Timestamp is null and cl.ParentId = @ChannelId

  ) as AllPosts
  ) as PagedPosts
  where PagedPosts.RowNum BETWEEN (@FromIndex+1) AND (@FromIndex+@PageSize)
  order by PagedPosts.RowNum

end